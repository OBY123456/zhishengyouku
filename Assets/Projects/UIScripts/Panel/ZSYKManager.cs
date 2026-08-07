using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BestHTTP;
using Lean.Pool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;

public class ZSYKManager : MonoBehaviour
{
    private const string HttpSeverUri = "https://api.itaocow.com.cn/youku/";
    private const string LoginUrl = "api/Auth/CreateLoginQrCode";
    private const string LoginResultUrl = "api/Auth/PollLogin";
    private const string TypeListUrl = "api/YouKu/GetCategoryList";
    private const string GameListUrl = "api/YouKu/GetProductPageList";
    private const int GamePageSize = 50;
    private const int GamePageIndex = 0;
    private const float NetworkRetryDelay = 2f;
    private const string NetworkKeyCreateQr = "CreateLoginQrCode";
    private const string NetworkKeyQrImage = "LoginQrImage";
    private const string NetworkKeyPollLogin = "PollLogin";
    private const string NetworkKeyCategory = "GetCategoryList";
    private const string NetworkKeyGameList = "GetProductPageList";
    private const string NetworkKeyCoverPrefix = "Cover:";

    #region 用户信息UI
    [Header("用户头像-默认未登录头像")]
    public Image UserAvatar;

    [Header("用户头像-登录后替换这个")]
    public Sprite LoginAvatar;

    [Header("用户ID，默认隐藏，登录后显示")]
    public Text UserID;

    [Header("未登录提示，登录后隐藏")]
    public GameObject NoLoginTips;

    #endregion

    #region 提示页UI
    [Header("提示页")]
    public CanvasGroup TipsPanel;

    [Header("网络状态提示")]
    public GameObject NetworkTips;

    [Header("二维码父物体")]
    public GameObject QRParent;

    [Header("登录二维码")]
    public RawImage QRCode;

    [Header("二维码加载动画")]
    public RectTransform CircleAnima;

    [Header("二维码提示")]
    public Text QRCodeTips;

    [Header("二维码加载动画旋转速度")]
    public float CircleRotateSpeed = 180f;

    [Header("是否打印全部接口返回数据")]
    public bool LogAllApiResponses = true;
    #endregion

    #region 游戏列表
    public GameObject TypePrefabs;
    public GameObject GameMsgPrefabs;
    public Transform TypeContent;
    public Transform GameMsgContent;
    #endregion

    [Header("格式为：版本1.0")]
    public Text Version;

    private string jwtStr = string.Empty;
    private string profileId = string.Empty;
    private Sprite defaultAvatar;
    private Texture defaultQrTexture;
    private Texture2D runtimeQrTexture;
    private HTTPRequest createQrRequest;
    private Coroutine qrImageCoroutine;
    private Coroutine typeLayoutCoroutine;
    private HTTPRequest pollLoginRequest;
    private HTTPRequest categoryRequest;
    private HTTPRequest gameListRequest;
    private Coroutine createQrRetryCoroutine;
    private Coroutine categoryRetryCoroutine;
    private Coroutine gameListRetryCoroutine;
    private InvokeInfo pollInvokeInfo;
    private InvokeInfo expireInvokeInfo;
    private bool pollRequestRunning;
    private int loginSession;
    private int gameRequestVersion;
    private TypeSet selectedType;
    private readonly List<TypeSet> typeItems = new List<TypeSet>();
    private readonly List<GameSet> gameItems = new List<GameSet>();
    private readonly Dictionary<string, Texture2D> coverTextureCache = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<Action<Texture2D>>> coverTextureCallbacks = new Dictionary<string, List<Action<Texture2D>>>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Coroutine> coverTextureCoroutines = new Dictionary<string, Coroutine>(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> networkFailureKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public bool IsLoggedIn { get { return !string.IsNullOrEmpty(jwtStr); } }

    [Serializable]
    public class CategoryData
    {
        public long Id;
        public string Name;
        public int Sort;
    }

    [Serializable]
    public class GameDisplayData
    {
        public string Id;
        public string Name;
        public string Description;
        public string Price;
        public string CoverUrl;
        public string DownloadUrl;
    }

    private void Awake()
    {
#if UNITY_STANDALONE_WIN
        Screen.SetResolution(Config.ProgramConfig.Instance.programData.分辨率X, Config.ProgramConfig.Instance.programData.分辨率Y, false);
#endif
        Application.targetFrameRate = 60;
        if (UserAvatar != null) defaultAvatar = UserAvatar.sprite;
        if (QRCode != null) defaultQrTexture = QRCode.texture;
    }

    private void Start()
    {
        if (Version != null) Version.text = "版本" + Application.version;
        SetLoggedOutUI();
        SetTipsPanelVisible(false);
        if (NetworkTips != null) NetworkTips.SetActive(false);
        if (QRCode != null) QRCode.gameObject.SetActive(true);
        SetQrParentVisible(false);
        ShowLoginPanel();
    }

    private void Update()
    {
        if (CircleAnima != null && CircleAnima.gameObject.activeInHierarchy) CircleAnima.Rotate(0f, 0f, -CircleRotateSpeed * Time.unscaledDeltaTime);
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
    }

    /// <summary>
    /// 程序启动时自动打开登录页，JWT失效时也会重新进入该流程。
    /// </summary>
    public void ShowLoginPanel()
    {
        if (IsLoggedIn || TipsPanel == null) return;
        StopLoginFlow();
        loginSession++;
        int session = loginSession;
        SetTipsPanelVisible(true);
        ReleaseRuntimeQrTexture();
        if (NetworkTips != null) NetworkTips.SetActive(false);
        if (QRCode != null) QRCode.texture = defaultQrTexture;
        SetQrParentVisible(true);
        SetQrLoading(true, "二维码加载中...");
        RequestLoginQrCode(session);
    }

    public void CloseLoginPanel()
    {
        if (!IsLoggedIn) return;
        loginSession++;
        StopLoginFlow();
        SetTipsPanelVisible(false);
        if (NetworkTips != null) NetworkTips.SetActive(false);
        SetQrParentVisible(false);
        ReleaseRuntimeQrTexture();
    }

    private void RequestLoginQrCode(int session)
    {
        StopRetryCoroutine(ref createQrRetryCoroutine);
        AbortRequest(ref createQrRequest);
        string url = HttpSeverUri + LoginUrl;
        createQrRequest = new HTTPRequest(new Uri(url), HTTPMethods.Post, (req, resp) =>
        {
            if (req == createQrRequest) createQrRequest = null;
            if (!IsLoginSessionValid(session)) return;
            PrintApiResponse("CreateLoginQrCode", url, null, req, resp);
            if (!IsHttpSuccess(req, resp))
            {
                if (IsRetryableNetworkFailure(req, resp))
                {
                    Debug.LogWarning("创建登录二维码网络请求失败，将在" + NetworkRetryDelay + "秒后重试。");
                    MarkNetworkRequestFailed(NetworkKeyCreateQr);
                    ScheduleCreateQrRetry(session);
                    return;
                }

                MarkNetworkRequestSucceeded(NetworkKeyCreateQr, true);
                HandleLoginRequestFailure("创建登录二维码", req, resp);
                return;
            }

            MarkNetworkRequestSucceeded(NetworkKeyCreateQr, true);
            JObject root;
            if (!TryParseObject(resp.DataAsText, "创建登录二维码", out root))
            {
                SetQrLoading(false, "服务器返回的二维码数据格式错误");
                return;
            }

            JObject data = GetObject(root, "Data");
            if (!IsApiSuccess(root) || data == null)
            {
                SetQrLoading(false, GetString(root, "Message", "Description", "登录二维码获取失败"));
                return;
            }

            string loginKey = GetString(data, "LoginKey", "loginKey");
            string qrContent = GetString(data, "QrCodeContent", "QrContent", "QrCodeText", "Content");
            string qrFullUrl = GetString(data, "QrCodeFullUrl");
            string qrRelativeUrl = GetString(data, "QrCodeUrl");
            int expiresSeconds = GetInt(data, 300, "ExpiresSeconds", "ExpireSeconds");
            if (string.IsNullOrEmpty(loginKey))
            {
                SetQrLoading(false, "登录二维码数据无效");
                return;
            }

            ScheduleQrExpiration(session, Mathf.Max(1, expiresSeconds));
            if (!string.IsNullOrEmpty(qrContent))
            {
                ShowGeneratedQrCode(qrContent);
                StartPollingLogin(loginKey, session);
                return;
            }

            List<string> qrImageUrls = BuildQrImageUrls(qrFullUrl, qrRelativeUrl);
            if (qrImageUrls.Count == 0)
            {
                SetQrLoading(false, "服务器未返回二维码");
                return;
            }

            StartQrImageDownload(qrImageUrls, loginKey, session);
        });
        createQrRequest.SetHeader("accept", "*/*");
        createQrRequest.RawData = new byte[0];
        createQrRequest.Send();
    }

    private void StartQrImageDownload(List<string> urls, string loginKey, int session)
    {
        StopQrImageDownload();
        qrImageCoroutine = StartCoroutine(DownloadQrImageCoroutine(urls, loginKey, session));
    }

    private IEnumerator DownloadQrImageCoroutine(List<string> urls, string loginKey, int session)
    {
        while (IsLoginSessionValid(session))
        {
            bool shouldRetry = false;
            for (int i = 0; i < urls.Count; i++)
            {
                if (!IsLoginSessionValid(session)) yield break;
                string url = urls[i];
                Debug.Log("尝试下载登录二维码：" + url);
                using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
                {
                    request.timeout = 15;
                    request.SetRequestHeader("Accept", "image/*");
                    yield return request.SendWebRequest();
                    if (!IsLoginSessionValid(session)) yield break;

                    if (IsUnityWebRequestSuccess(request))
                    {
                        MarkNetworkRequestSucceeded(NetworkKeyQrImage, true);
                        Texture2D texture = null;
                        try { texture = DownloadHandlerTexture.GetContent(request); }
                        catch (Exception e) { Debug.LogError("解析二维码图片失败：" + e.Message); }
                        if (texture != null)
                        {
                            Debug.Log("[二维码图片] 下载成功，HTTP=" + request.responseCode + "，Url=" + url);
                            qrImageCoroutine = null;
                            SetRuntimeQrTexture(texture);
                            SetQrLoading(false, "微信扫码登录");
                            StartPollingLogin(loginKey, session);
                            yield break;
                        }

                        Debug.LogError("二维码请求成功但图片解析失败，Url=" + url);
                        continue;
                    }

                    if (IsRetryableNetworkFailure(request))
                    {
                        Debug.LogWarning("二维码图片网络请求失败，将在" + NetworkRetryDelay + "秒后重试，HTTP=" + request.responseCode + "，Error=" + request.error + "，Url=" + url);
                        MarkNetworkRequestFailed(NetworkKeyQrImage);
                        shouldRetry = true;
                        break;
                    }

                    MarkNetworkRequestSucceeded(NetworkKeyQrImage, true);
                    Debug.LogWarning("二维码地址请求失败且不可重试，HTTP=" + request.responseCode + "，Error=" + request.error + "，Url=" + url);
                }
            }

            if (!shouldRetry)
            {
                qrImageCoroutine = null;
                SetQrLoading(false, "二维码资源不存在，请检查服务端返回地址");
                yield break;
            }

            yield return new WaitForSecondsRealtime(NetworkRetryDelay);
        }

        qrImageCoroutine = null;
    }

    private void StopQrImageDownload()
    {
        if (qrImageCoroutine == null) return;
        StopCoroutine(qrImageCoroutine);
        qrImageCoroutine = null;
    }

    private void ShowGeneratedQrCode(string content)
    {
        if (QRCode == null) return;
        int width = Mathf.Max(256, Mathf.RoundToInt(QRCode.rectTransform.rect.width));
        int height = Mathf.Max(256, Mathf.RoundToInt(QRCode.rectTransform.rect.height));
        QrCodeEncodingOptions options = new QrCodeEncodingOptions();
        options.CharacterSet = "UTF-8";
        options.DisableECI = true;
        options.ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.H;
        options.Width = width;
        options.Height = height;
        options.Margin = 1;
        BarcodeWriter writer = new BarcodeWriter();
        writer.Format = BarcodeFormat.QR_CODE;
        writer.Options = options;
        Color32[] colors = writer.Write(content);
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false, false);
        texture.SetPixels32(colors);
        texture.Apply();
        SetRuntimeQrTexture(texture);
        SetQrLoading(false, "微信扫码登录");
    }

    private void StartPollingLogin(string loginKey, int session)
    {
        if (!IsLoginSessionValid(session)) return;
        PollLogin(loginKey, session);
        RemoveInvoke(ref pollInvokeInfo);
        pollInvokeInfo = InvokeUtil.Instance.Run(() => { PollLogin(loginKey, session); }, 1f, 0);
    }

    private void PollLogin(string loginKey, int session)
    {
        if (!IsLoginSessionValid(session) || pollRequestRunning) return;
        pollRequestRunning = true;
        string url = HttpSeverUri + LoginResultUrl + "?loginKey=" + Uri.EscapeDataString(loginKey);
        pollLoginRequest = new HTTPRequest(new Uri(url), HTTPMethods.Get, (req, resp) =>
        {
            pollRequestRunning = false;
            if (req == pollLoginRequest) pollLoginRequest = null;
            if (!IsLoginSessionValid(session)) return;
            PrintApiResponse("PollLogin", url, null, req, resp);
            if (!IsHttpSuccess(req, resp))
            {
                if (IsRetryableNetworkFailure(req, resp))
                {
                    MarkNetworkRequestFailed(NetworkKeyPollLogin);
                    return;
                }

                MarkNetworkRequestSucceeded(NetworkKeyPollLogin, true);
                if (IsUnauthorized(resp)) HandleAuthorizationExpired();
                else Debug.LogError("轮询登录失败，HTTP=" + (resp == null ? 0 : resp.StatusCode));
                return;
            }

            MarkNetworkRequestSucceeded(NetworkKeyPollLogin, true);
            JObject root;
            if (!TryParseObject(resp.DataAsText, "轮询登录", out root)) return;
            JObject data = GetObject(root, "Data");
            if (!IsApiSuccess(root) || data == null) return;
            string status = GetString(data, "Status");
            if (string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase)) return;
            if (string.Equals(status, "Expired", StringComparison.OrdinalIgnoreCase))
            {
                SetLoginExpired();
                return;
            }
            if (!string.Equals(status, "Success", StringComparison.OrdinalIgnoreCase)) return;

            string token = NormalizeToken(GetString(data, "JwtStr", "Token", "AccessToken"));
            string id = GetString(data, "ProfileId", "UserId", "Id");
            if (string.IsNullOrEmpty(token))
            {
                SetQrLoading(false, "登录成功，但未返回JWT");
                return;
            }
            FinishLogin(token, id);
        });
        pollLoginRequest.SetHeader("accept", "*/*");
        pollLoginRequest.Send();
    }

    private void FinishLogin(string token, string id)
    {
        jwtStr = token;
        profileId = id;
        SetLoggedInUI();
        loginSession++;
        StopLoginFlow();
        SetTipsPanelVisible(false);
        if (NetworkTips != null) NetworkTips.SetActive(false);
        SetQrParentVisible(false);
        ReleaseRuntimeQrTexture();
        LoadCategoryList();
    }

    private void ScheduleQrExpiration(int session, int expiresSeconds)
    {
        RemoveInvoke(ref expireInvokeInfo);
        expireInvokeInfo = InvokeUtil.Instance.Run(() =>
        {
            if (!IsLoginSessionValid(session)) return;
            SetLoginExpired();
        }, expiresSeconds);
    }

    private void SetLoginExpired()
    {
        AbortRequest(ref pollLoginRequest);
        pollRequestRunning = false;
        RemoveInvoke(ref pollInvokeInfo);
        RemoveInvoke(ref expireInvokeInfo);
        StopQrImageDownload();
        RemoveNetworkIssue(NetworkKeyPollLogin);
        RemoveNetworkIssue(NetworkKeyQrImage);
        ReleaseRuntimeQrTexture();
        loginSession++;
        int session = loginSession;
        SetTipsPanelVisible(true);
        if (NetworkTips != null) NetworkTips.SetActive(false);
        SetQrParentVisible(true);
        SetQrLoading(true, "二维码已过期，正在自动刷新...");
        RequestLoginQrCode(session);
    }

    private void LoadCategoryList()
    {
        if (!IsLoggedIn) return;
        StopRetryCoroutine(ref categoryRetryCoroutine);
        AbortRequest(ref categoryRequest);
        AbortRequest(ref gameListRequest);
        ClearTypeItems();
        ClearGameItems();
        string url = HttpSeverUri + TypeListUrl;
        categoryRequest = new HTTPRequest(new Uri(url), HTTPMethods.Post, (req, resp) =>
        {
            if (req == categoryRequest) categoryRequest = null;
            if (!IsLoggedIn) return;
            PrintApiResponse("GetCategoryList", url, null, req, resp);
            if (!IsHttpSuccess(req, resp))
            {
                if (IsRetryableNetworkFailure(req, resp))
                {
                    Debug.LogWarning("获取类别列表网络请求失败，将在" + NetworkRetryDelay + "秒后重试。");
                    MarkNetworkRequestFailed(NetworkKeyCategory);
                    ScheduleCategoryRetry();
                    return;
                }

                MarkNetworkRequestSucceeded(NetworkKeyCategory, false);
                if (IsUnauthorized(resp)) HandleAuthorizationExpired();
                else Debug.LogError("获取类别列表失败，HTTP=" + (resp == null ? 0 : resp.StatusCode));
                return;
            }

            MarkNetworkRequestSucceeded(NetworkKeyCategory, false);
            JObject root;
            if (!TryParseObject(resp.DataAsText, "获取类别列表", out root) || !IsApiSuccess(root)) return;
            JArray data = GetArray(root, "Data");
            if (data == null || data.Count == 0) return;

            List<CategoryData> categories = new List<CategoryData>();
            for (int i = 0; i < data.Count; i++)
            {
                JObject item = data[i] as JObject;
                if (item == null) continue;
                long id = GetLong(item, 0L, "Id", "CategoryId");
                if (id == 0L) continue;
                CategoryData category = new CategoryData();
                category.Id = id;
                category.Name = GetString(item, "Name", "CategoryName");
                category.Sort = GetInt(item, i, "Sort");
                categories.Add(category);
            }

            categories.Sort((a, b) => a.Sort.CompareTo(b.Sort));
            Debug.Log("[GetCategoryList] 解析后的类别数量=" + categories.Count);
            for (int i = 0; i < categories.Count; i++)
            {
                Debug.Log("[GetCategoryList] 解析类别[" + i + "] Id=" + categories[i].Id + "，Name=" + categories[i].Name + "，Sort=" + categories[i].Sort);
                CreateTypeItem(categories[i]);
            }
            RefreshTypeLayout();
            StartTypeLayoutRefresh();
            if (typeItems.Count > 0) SelectType(typeItems[0], true);
        });
        categoryRequest.SetHeader("accept", "*/*");
        categoryRequest.SetHeader("Authorization", "Bearer " + jwtStr);
        categoryRequest.RawData = new byte[0];
        categoryRequest.Send();
    }

    private void CreateTypeItem(CategoryData data)
    {
        if (TypePrefabs == null || TypeContent == null) return;
        GameObject obj = LeanPool.Spawn(TypePrefabs, TypeContent);
        obj.transform.SetParent(TypeContent, false);
        obj.transform.localScale = Vector3.one;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.SetAsLastSibling();
        RectTransform rect = obj.transform as RectTransform;
        if (rect != null) rect.anchoredPosition3D = Vector3.zero;
        obj.SetActive(true);
        TypeSet item = obj.GetComponent<TypeSet>();
        if (item == null)
        {
            Debug.LogError("TypePrefabs缺少TypeSet组件。");
            LeanPool.Despawn(obj);
            return;
        }
        item.Bind(data.Id, data.Name, OnTypeClicked);
        typeItems.Add(item);
    }

    private void StartTypeLayoutRefresh()
    {
        if (typeLayoutCoroutine != null) StopCoroutine(typeLayoutCoroutine);
        typeLayoutCoroutine = StartCoroutine(RefreshTypeLayoutNextFrame());
    }

    private IEnumerator RefreshTypeLayoutNextFrame()
    {
        yield return null;
        RefreshTypeLayout();
        yield return new WaitForEndOfFrame();
        RefreshTypeLayout();
        typeLayoutCoroutine = null;
    }

    private void RefreshTypeLayout()
    {
        RectTransform contentRect = TypeContent as RectTransform;
        if (contentRect == null) return;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.MarkLayoutForRebuild(contentRect);
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        Canvas.ForceUpdateCanvases();
    }

    private void StopTypeLayoutRefresh()
    {
        if (typeLayoutCoroutine == null) return;
        StopCoroutine(typeLayoutCoroutine);
        typeLayoutCoroutine = null;
    }

    private void OnTypeClicked(TypeSet item)
    {
        if (item == null || item == selectedType) return;
        SelectType(item, false);
    }

    private void SelectType(TypeSet item, bool isDefault)
    {
        if (item == null || (!isDefault && item == selectedType)) return;
        if (selectedType != null) selectedType.SetSelected(false);
        selectedType = item;
        selectedType.SetSelected(true);
        LoadGameList(selectedType.CategoryId);
    }

    private void LoadGameList(long categoryId)
    {
        if (!IsLoggedIn) return;
        StopRetryCoroutine(ref gameListRetryCoroutine);
        AbortRequest(ref gameListRequest);
        ClearGameItems();
        int requestVersion = ++gameRequestVersion;
        JObject body = new JObject();
        body["pageSize"] = GamePageSize;
        body["pageIndex"] = GamePageIndex;
        body["kw"] = string.Empty;
        body["categoryId"] = categoryId;
        string requestBody = body.ToString(Formatting.None);
        string url = HttpSeverUri + GameListUrl;
        gameListRequest = new HTTPRequest(new Uri(url), HTTPMethods.Post, (req, resp) =>
        {
            if (req == gameListRequest) gameListRequest = null;
            if (!IsLoggedIn || requestVersion != gameRequestVersion || selectedType == null || selectedType.CategoryId != categoryId) return;
            PrintApiResponse("GetProductPageList，CategoryId=" + categoryId, url, requestBody, req, resp);
            if (!IsHttpSuccess(req, resp))
            {
                if (IsRetryableNetworkFailure(req, resp))
                {
                    Debug.LogWarning("获取游戏列表网络请求失败，将在" + NetworkRetryDelay + "秒后重试，CategoryId=" + categoryId);
                    MarkNetworkRequestFailed(NetworkKeyGameList);
                    ScheduleGameListRetry(categoryId);
                    return;
                }

                MarkNetworkRequestSucceeded(NetworkKeyGameList, false);
                if (IsUnauthorized(resp)) HandleAuthorizationExpired();
                else Debug.LogError("获取游戏列表失败，HTTP=" + (resp == null ? 0 : resp.StatusCode));
                return;
            }

            MarkNetworkRequestSucceeded(NetworkKeyGameList, false);
            JObject root;
            if (!TryParseObject(resp.DataAsText, "获取游戏列表", out root) || !IsApiSuccess(root)) return;
            JArray data = GetArray(root, "Data");
            int total = GetInt(root, data == null ? 0 : data.Count, "Total");
            Debug.Log("[GetProductPageList] CategoryId=" + categoryId + "，Total=" + total + "，本次Data数量=" + (data == null ? 0 : data.Count));
            if (total > GamePageSize) Debug.LogWarning("当前类别游戏总数超过50条，接口本次只请求前50条。");
            if (data == null) return;
            for (int i = 0; i < data.Count; i++)
            {
                JObject item = data[i] as JObject;
                if (item == null) continue;
                CreateGameItem(ParseGameData(item));
            }
        });
        gameListRequest.SetHeader("accept", "*/*");
        gameListRequest.SetHeader("Authorization", "Bearer " + jwtStr);
        gameListRequest.SetHeader("Content-Type", "application/json-patch+json");
        gameListRequest.RawData = Encoding.UTF8.GetBytes(requestBody);
        gameListRequest.Send();
    }

    private GameDisplayData ParseGameData(JObject item)
    {
        GameDisplayData data = new GameDisplayData();
        data.Id = GetString(item, "Id", "ProductId", "GameId");
        data.Name = GetString(item, "Name", "ProductName", "GameName", "Title");
        data.Description = CleanDescription(GetString(item, "Description", "ProductDescription", "GameDescription", "Desc", "Introduce", "Summary"));
        data.Price = GetString(item, "Price", "ProductPrice", "GamePrice", "SalePrice");
        JProperty coverProperty = FindProperty(item, "CoverUrl", "Cover", "ImageUrl", "Image", "ProductImage", "ProductImageUrl", "MainImage", "Icon");
        JProperty downloadProperty = FindProperty(item, "DownloadUrl", "DownloadURL", "DownloadAddress", "DownUrl");
        JToken coverToken = coverProperty == null ? null : coverProperty.Value;
        JToken downloadToken = downloadProperty == null ? null : downloadProperty.Value;
        string rawCoverUrl = GetFirstString(coverToken);
        string rawDownloadUrl = GetFirstString(downloadToken);
        data.CoverUrl = BuildAbsoluteUrl(rawCoverUrl);
        data.DownloadUrl = BuildAbsoluteUrl(rawDownloadUrl);
        if (LogAllApiResponses) PrintGameResourceAddress(data, item, coverProperty, rawCoverUrl, downloadProperty, rawDownloadUrl);
        return data;
    }

    private void PrintGameResourceAddress(GameDisplayData data, JObject item, JProperty coverProperty, string rawCoverUrl, JProperty downloadProperty, string rawDownloadUrl)
    {
        Debug.Log("========== 游戏资源地址 ==========");
        Debug.Log("[游戏资源] 游戏ID：" + EmptyText(data.Id));
        Debug.Log("[游戏资源] 游戏名称：" + EmptyText(data.Name));
        Debug.Log("[游戏资源] 预览图原始字段：" + (coverProperty == null ? "<未找到对应字段>" : coverProperty.Name));
        Debug.Log("[游戏资源] 预览图原始值：" + (coverProperty == null ? "<空>" : coverProperty.Value.ToString(Formatting.None)));
        Debug.Log("[游戏资源] 预览图提取地址：" + EmptyText(rawCoverUrl));
        Debug.Log("[游戏资源] 预览图最终地址：" + EmptyText(data.CoverUrl));
        Debug.Log("[游戏资源] 下载地址原始字段：" + (downloadProperty == null ? "<未找到对应字段>" : downloadProperty.Name));
        Debug.Log("[游戏资源] 下载地址原始值：" + (downloadProperty == null ? "<空>" : downloadProperty.Value.ToString(Formatting.None)));
        Debug.Log("[游戏资源] 下载地址提取值：" + EmptyText(rawDownloadUrl));
        Debug.Log("[游戏资源] 下载地址最终值：" + EmptyText(data.DownloadUrl));
        Debug.Log("[游戏资源] 完整游戏数据：" + item.ToString(Formatting.None));
        Debug.Log("================================");
    }

    private static string EmptyText(string value)
    {
        return string.IsNullOrEmpty(value) ? "<空>" : value;
    }

    public void RequestCoverTexture(string url, Action<Texture2D> callback)
    {
        if (callback == null) return;
        if (string.IsNullOrWhiteSpace(url))
        {
            callback(null);
            return;
        }

        Texture2D cachedTexture;
        if (coverTextureCache.TryGetValue(url, out cachedTexture))
        {
            if (cachedTexture != null)
            {
                Debug.Log("[游戏封面缓存] 命中缓存，不再请求，Url=" + url);
                callback(cachedTexture);
                return;
            }
            coverTextureCache.Remove(url);
        }

        List<Action<Texture2D>> callbacks;
        if (coverTextureCallbacks.TryGetValue(url, out callbacks))
        {
            callbacks.Add(callback);
            Debug.Log("[游戏封面缓存] 已有相同地址正在下载，加入等待队列，Url=" + url);
            return;
        }

        callbacks = new List<Action<Texture2D>>();
        callbacks.Add(callback);
        coverTextureCallbacks.Add(url, callbacks);
        Coroutine coroutine = StartCoroutine(DownloadAndCacheCoverCoroutine(url));
        coverTextureCoroutines[url] = coroutine;
    }

    private IEnumerator DownloadAndCacheCoverCoroutine(string url)
    {
        Texture2D texture = null;
        string networkKey = NetworkKeyCoverPrefix + url;
        while (true)
        {
            Debug.Log("[游戏封面缓存] 请求图片，Url=" + url);
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
            {
                request.timeout = 20;
                request.SetRequestHeader("Accept", "image/*");
                yield return request.SendWebRequest();

                if (IsUnityWebRequestSuccess(request))
                {
                    MarkNetworkRequestSucceeded(networkKey, false);
                    try { texture = DownloadHandlerTexture.GetContent(request); }
                    catch (Exception e) { Debug.LogError("[游戏封面缓存] 图片解析失败：" + e.Message + "，Url=" + url); }

                    if (texture != null)
                    {
                        texture.name = "CachedCover_" + texture.GetInstanceID();
                        coverTextureCache[url] = texture;
                        Debug.Log("[游戏封面缓存] 下载并缓存成功，Size=" + texture.width + "x" + texture.height + "，当前缓存数量=" + coverTextureCache.Count + "，Url=" + url);
                    }
                    else
                    {
                        Debug.LogError("[游戏封面缓存] 请求成功但Texture2D为空，Url=" + url);
                    }
                    break;
                }

                if (IsRetryableNetworkFailure(request))
                {
                    Debug.LogWarning("[游戏封面缓存] 网络请求失败，将在" + NetworkRetryDelay + "秒后重试，HTTP=" + request.responseCode + "，Error=" + request.error + "，Url=" + url);
                    MarkNetworkRequestFailed(networkKey);
                    yield return new WaitForSecondsRealtime(NetworkRetryDelay);
                    continue;
                }

                MarkNetworkRequestSucceeded(networkKey, false);
                Debug.LogError("[游戏封面缓存] 下载失败且不可重试，HTTP=" + request.responseCode + "，Error=" + request.error + "，Url=" + url);
                break;
            }
        }

        coverTextureCoroutines.Remove(url);
        List<Action<Texture2D>> callbacks;
        if (!coverTextureCallbacks.TryGetValue(url, out callbacks)) yield break;
        coverTextureCallbacks.Remove(url);
        for (int i = 0; i < callbacks.Count; i++)
        {
            try { callbacks[i](texture); }
            catch (Exception e) { Debug.LogError("[游戏封面缓存] 回调执行失败：" + e.Message); }
        }
    }

    private void ClearCoverTextureCache()
    {
        foreach (KeyValuePair<string, Coroutine> pair in coverTextureCoroutines)
        {
            if (pair.Value != null) StopCoroutine(pair.Value);
        }
        coverTextureCoroutines.Clear();
        coverTextureCallbacks.Clear();

        foreach (KeyValuePair<string, Texture2D> pair in coverTextureCache)
        {
            if (pair.Value != null) Destroy(pair.Value);
        }
        coverTextureCache.Clear();
        RemoveNetworkIssuesWithPrefix(NetworkKeyCoverPrefix);
    }

    private void CreateGameItem(GameDisplayData data)
    {
        if (GameMsgPrefabs == null || GameMsgContent == null) return;
        GameObject obj = LeanPool.Spawn(GameMsgPrefabs, GameMsgContent);
        obj.SetActive(true);
        GameSet item = obj.GetComponent<GameSet>();
        if (item == null)
        {
            Debug.LogError("GameMsgPrefabs缺少GameSet组件。");
            LeanPool.Despawn(obj);
            return;
        }
        item.Bind(this, data);
        gameItems.Add(item);
    }

    private void HandleAuthorizationExpired()
    {
        StopRetryCoroutine(ref categoryRetryCoroutine);
        StopRetryCoroutine(ref gameListRetryCoroutine);
        AbortRequest(ref categoryRequest);
        AbortRequest(ref gameListRequest);
        RemoveNetworkIssue(NetworkKeyCategory);
        RemoveNetworkIssue(NetworkKeyGameList);
        jwtStr = string.Empty;
        profileId = string.Empty;
        SetLoggedOutUI();
        ClearTypeItems();
        ClearGameItems();
        ClearCoverTextureCache();
        ShowLoginPanel();
    }

    private void SetLoggedInUI()
    {
        if (UserID != null)
        {
            UserID.text = profileId;
            UserID.gameObject.SetActive(true);
        }
        if (UserAvatar != null && LoginAvatar != null) UserAvatar.sprite = LoginAvatar;
        if (NoLoginTips != null) NoLoginTips.SetActive(false);
    }

    private void SetLoggedOutUI()
    {
        if (UserID != null)
        {
            UserID.text = string.Empty;
            UserID.gameObject.SetActive(false);
        }
        if (UserAvatar != null && defaultAvatar != null) UserAvatar.sprite = defaultAvatar;
        if (NoLoginTips != null) NoLoginTips.SetActive(true);
    }

    private void PrintApiResponse(string apiName, string url, string requestBody, HTTPRequest request, HTTPResponse response)
    {
        if (!LogAllApiResponses) return;
        string state = request == null ? "null" : request.State.ToString();
        int statusCode = response == null ? 0 : response.StatusCode;
        string responseBody = response == null ? string.Empty : response.DataAsText;
        string requestText = string.IsNullOrEmpty(requestBody) ? string.Empty : "\nRequestBody:\n" + requestBody;
        Debug.Log("========== API返回 ==========" + "\n接口：" + apiName + "\nURL：" + url + requestText + "\nState：" + state + "\nHTTP：" + statusCode + "\nResponseBody：\n" + responseBody + "\n============================");
    }

    private void MarkNetworkRequestFailed(string key)
    {
        if (!string.IsNullOrEmpty(key)) networkFailureKeys.Add(key);
        ShowNetworkTips();
    }

    private void MarkNetworkRequestSucceeded(string key, bool loginFlow)
    {
        RemoveNetworkIssue(key);
        if (networkFailureKeys.Count > 0) return;
        if (NetworkTips != null) NetworkTips.SetActive(false);
        if (loginFlow && !IsLoggedIn)
        {
            SetTipsPanelVisible(true);
            SetQrParentVisible(true);
            return;
        }
        if (IsLoggedIn)
        {
            SetQrParentVisible(false);
            SetTipsPanelVisible(false);
        }
    }

    private void RemoveNetworkIssue(string key)
    {
        if (!string.IsNullOrEmpty(key)) networkFailureKeys.Remove(key);
    }

    private void RemoveNetworkIssuesWithPrefix(string prefix)
    {
        if (string.IsNullOrEmpty(prefix) || networkFailureKeys.Count == 0) return;
        List<string> keys = new List<string>(networkFailureKeys);
        for (int i = 0; i < keys.Count; i++)
        {
            if (keys[i].StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) networkFailureKeys.Remove(keys[i]);
        }
        if (networkFailureKeys.Count == 0 && NetworkTips != null) NetworkTips.SetActive(false);
    }

    private void ScheduleCreateQrRetry(int session)
    {
        StopRetryCoroutine(ref createQrRetryCoroutine);
        createQrRetryCoroutine = StartCoroutine(RetryCreateQrCoroutine(session));
    }

    private IEnumerator RetryCreateQrCoroutine(int session)
    {
        yield return new WaitForSecondsRealtime(NetworkRetryDelay);
        createQrRetryCoroutine = null;
        if (IsLoginSessionValid(session) && !IsLoggedIn) RequestLoginQrCode(session);
    }

    private void ScheduleCategoryRetry()
    {
        StopRetryCoroutine(ref categoryRetryCoroutine);
        categoryRetryCoroutine = StartCoroutine(RetryCategoryCoroutine());
    }

    private IEnumerator RetryCategoryCoroutine()
    {
        yield return new WaitForSecondsRealtime(NetworkRetryDelay);
        categoryRetryCoroutine = null;
        if (IsLoggedIn) LoadCategoryList();
    }

    private void ScheduleGameListRetry(long categoryId)
    {
        StopRetryCoroutine(ref gameListRetryCoroutine);
        gameListRetryCoroutine = StartCoroutine(RetryGameListCoroutine(categoryId));
    }

    private IEnumerator RetryGameListCoroutine(long categoryId)
    {
        yield return new WaitForSecondsRealtime(NetworkRetryDelay);
        gameListRetryCoroutine = null;
        if (IsLoggedIn && selectedType != null && selectedType.CategoryId == categoryId) LoadGameList(categoryId);
    }

    private void StopRetryCoroutine(ref Coroutine coroutine)
    {
        if (coroutine == null) return;
        StopCoroutine(coroutine);
        coroutine = null;
    }

    private static bool IsRetryableNetworkFailure(HTTPRequest request, HTTPResponse response)
    {
        if (request == null) return true;
        if (request.State == HTTPRequestStates.Aborted) return false;
        if (request.State == HTTPRequestStates.Error || request.State == HTTPRequestStates.ConnectionTimedOut || request.State == HTTPRequestStates.TimedOut) return true;
        if (response == null) return true;
        return response.StatusCode == 0 || response.StatusCode == 408 || response.StatusCode == 429 || response.StatusCode >= 500;
    }

    private static bool IsUnityWebRequestSuccess(UnityWebRequest request)
    {
        return request != null && !request.isNetworkError && !request.isHttpError && request.responseCode >= 200 && request.responseCode < 300;
    }

    private static bool IsRetryableNetworkFailure(UnityWebRequest request)
    {
        if (request == null) return true;
        return request.isNetworkError || request.responseCode == 0 || request.responseCode == 408 || request.responseCode == 429 || request.responseCode >= 500;
    }

    private void HandleLoginRequestFailure(string requestName, HTTPRequest request, HTTPResponse response)
    {
        if (request == null || request.State == HTTPRequestStates.Aborted) return;
        int statusCode = response == null ? 0 : response.StatusCode;
        string responseText = response == null ? string.Empty : response.DataAsText;
        Debug.LogError(requestName + "失败，State=" + request.State + "，HTTP=" + statusCode + "，Response=" + responseText);
        SetQrLoading(false, requestName + "失败（HTTP " + statusCode + "）");
    }

    private void ShowNetworkTips()
    {
        SetQrParentVisible(false);
        SetTipsPanelVisible(true);
        if (CircleAnima != null) CircleAnima.gameObject.SetActive(false);
        if (NetworkTips != null) NetworkTips.SetActive(true);
    }

    private void SetQrLoading(bool loading, string text)
    {
        if (CircleAnima != null) CircleAnima.gameObject.SetActive(loading);
        if (QRCodeTips != null)
        {
            QRCodeTips.gameObject.SetActive(true);
            QRCodeTips.text = text;
        }
    }

    private void SetTipsPanelVisible(bool visible)
    {
        if (TipsPanel == null) return;
        TipsPanel.gameObject.SetActive(visible);
        TipsPanel.alpha = visible ? 1f : 0f;
        TipsPanel.interactable = visible;
        TipsPanel.blocksRaycasts = visible;
    }

    private bool IsLoginSessionValid(int session)
    {
        return session == loginSession && TipsPanel != null && TipsPanel.gameObject.activeInHierarchy && TipsPanel.blocksRaycasts;
    }

    private void StopLoginFlow()
    {
        AbortRequest(ref createQrRequest);
        StopRetryCoroutine(ref createQrRetryCoroutine);
        StopQrImageDownload();
        AbortRequest(ref pollLoginRequest);
        pollRequestRunning = false;
        RemoveInvoke(ref pollInvokeInfo);
        RemoveInvoke(ref expireInvokeInfo);
        RemoveNetworkIssue(NetworkKeyCreateQr);
        RemoveNetworkIssue(NetworkKeyQrImage);
        RemoveNetworkIssue(NetworkKeyPollLogin);
    }

    private void ClearTypeItems()
    {
        StopTypeLayoutRefresh();
        selectedType = null;
        for (int i = typeItems.Count - 1; i >= 0; i--)
        {
            if (typeItems[i] != null) LeanPool.Despawn(typeItems[i].gameObject);
        }
        typeItems.Clear();
    }

    private void ClearGameItems()
    {
        gameRequestVersion++;
        for (int i = gameItems.Count - 1; i >= 0; i--)
        {
            if (gameItems[i] != null) LeanPool.Despawn(gameItems[i].gameObject);
        }
        gameItems.Clear();
    }

    private void SetRuntimeQrTexture(Texture2D texture)
    {
        ReleaseRuntimeQrTexture();
        runtimeQrTexture = texture;
        if (QRCode != null) QRCode.texture = runtimeQrTexture;
        SetQrParentVisible(true);
    }

    private void SetQrParentVisible(bool visible)
    {
        if (QRParent != null)
        {
            QRParent.SetActive(visible);
            return;
        }
        if (QRCode != null) QRCode.gameObject.SetActive(visible);
    }

    private void ReleaseRuntimeQrTexture()
    {
        if (runtimeQrTexture != null)
        {
            Destroy(runtimeQrTexture);
            runtimeQrTexture = null;
        }
        if (QRCode != null) QRCode.texture = defaultQrTexture;
    }

    private static bool IsHttpSuccess(HTTPRequest request, HTTPResponse response)
    {
        return request != null && request.State == HTTPRequestStates.Finished && response != null && response.StatusCode >= 200 && response.StatusCode < 300;
    }

    private static bool IsUnauthorized(HTTPResponse response)
    {
        return response != null && response.StatusCode == 401;
    }

    private static void AbortRequest(ref HTTPRequest request)
    {
        if (request == null) return;
        try { request.Abort(); }
        catch { }
        request = null;
    }

    private static void RemoveInvoke(ref InvokeInfo info)
    {
        if (info == null) return;
        InvokeUtil.Instance.Remove(info);
        info = null;
    }

    private static bool TryParseObject(string json, string apiName, out JObject result)
    {
        result = null;
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError(apiName + "返回空数据。");
            return false;
        }
        try
        {
            result = JObject.Parse(json);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(apiName + "JSON解析失败：" + e.Message + "\n" + json);
            return false;
        }
    }

    private static bool IsApiSuccess(JObject root)
    {
        if (root == null) return false;
        JToken tag = FindToken(root, "Tag");
        if (tag == null) return true;
        int value;
        return int.TryParse(tag.ToString(), out value) && value == 1;
    }

    private static JObject GetObject(JObject root, params string[] names)
    {
        return FindToken(root, names) as JObject;
    }

    private static JArray GetArray(JObject root, params string[] names)
    {
        return FindToken(root, names) as JArray;
    }

    private static string GetString(JObject root, params string[] names)
    {
        return GetFirstString(FindToken(root, names));
    }

    private static string GetString(JObject root, string name1, string name2, string defaultValue)
    {
        string value = GetString(root, name1, name2);
        return string.IsNullOrEmpty(value) ? defaultValue : value;
    }

    private static int GetInt(JObject root, int defaultValue, params string[] names)
    {
        JToken token = FindToken(root, names);
        int value;
        return token != null && int.TryParse(token.ToString(), out value) ? value : defaultValue;
    }

    private static long GetLong(JObject root, long defaultValue, params string[] names)
    {
        JToken token = FindToken(root, names);
        long value;
        return token != null && long.TryParse(token.ToString(), out value) ? value : defaultValue;
    }

    private static JProperty FindProperty(JObject root, params string[] names)
    {
        if (root == null || names == null) return null;
        foreach (JProperty property in root.Properties())
        {
            for (int i = 0; i < names.Length; i++)
            {
                if (string.Equals(property.Name, names[i], StringComparison.OrdinalIgnoreCase)) return property;
            }
        }
        return null;
    }

    private static JToken FindToken(JObject root, params string[] names)
    {
        JProperty property = FindProperty(root, names);
        return property == null ? null : property.Value;
    }

    private static string GetFirstString(JToken token)
    {
        if (token == null || token.Type == JTokenType.Null || token.Type == JTokenType.Undefined) return string.Empty;
        if (token is JValue) return token.ToString();
        JArray array = token as JArray;
        if (array != null)
        {
            for (int i = 0; i < array.Count; i++)
            {
                string value = GetFirstString(array[i]);
                if (!string.IsNullOrEmpty(value)) return value;
            }
            return string.Empty;
        }
        JObject obj = token as JObject;
        if (obj != null)
        {
            JToken preferred = FindToken(obj, "Url", "ImageUrl", "Path", "Value", "DownloadUrl");
            if (preferred != null) return GetFirstString(preferred);
            foreach (JProperty property in obj.Properties())
            {
                string value = GetFirstString(property.Value);
                if (!string.IsNullOrEmpty(value)) return value;
            }
        }
        return string.Empty;
    }

    private static string CleanDescription(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        value = System.Text.RegularExpressions.Regex.Replace(value, @"</?p(?:\s[^>]*)?>", string.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return System.Net.WebUtility.HtmlDecode(value).Trim();
    }

    private static string NormalizeToken(string token)
    {
        if (string.IsNullOrEmpty(token)) return string.Empty;
        token = token.Trim();
        return token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? token.Substring(7).Trim() : token;
    }

    private static List<string> BuildQrImageUrls(string fullUrl, string relativeUrl)
    {
        List<string> urls = new List<string>();
        AddQrImageUrlCandidates(urls, fullUrl);
        AddQrImageUrlCandidates(urls, relativeUrl);
        return urls;
    }

    private static void AddQrImageUrlCandidates(List<string> urls, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        value = value.Trim().Replace("\\", "/");
        Uri absolute;
        if (Uri.TryCreate(value, UriKind.Absolute, out absolute))
        {
            AddUniqueUrl(urls, absolute.AbsoluteUri);
            AddUniqueUrl(urls, GetServerOrigin() + absolute.PathAndQuery);
            AddUniqueUrl(urls, HttpSeverUri.TrimEnd('/') + "/" + absolute.PathAndQuery.TrimStart('/'));
            return;
        }

        AddUniqueUrl(urls, BuildAbsoluteUrl(value));
        AddUniqueUrl(urls, GetServerOrigin() + "/" + value.TrimStart('/'));
        AddUniqueUrl(urls, HttpSeverUri.TrimEnd('/') + "/" + value.TrimStart('/'));
    }

    private static void AddUniqueUrl(List<string> urls, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        for (int i = 0; i < urls.Count; i++)
        {
            if (string.Equals(urls[i], value, StringComparison.OrdinalIgnoreCase)) return;
        }
        urls.Add(value);
    }

    private static string GetServerOrigin()
    {
        try { return new Uri(HttpSeverUri).GetLeftPart(UriPartial.Authority); }
        catch { return HttpSeverUri.TrimEnd('/'); }
    }

    private static string BuildAbsoluteUrl(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        value = value.Trim();
        Uri absolute;
        if (Uri.TryCreate(value, UriKind.Absolute, out absolute)) return absolute.AbsoluteUri;
        try { return new Uri(new Uri(HttpSeverUri), value).AbsoluteUri; }
        catch { return value; }
    }

    private void OnDestroy()
    {
        StopLoginFlow();
        StopRetryCoroutine(ref categoryRetryCoroutine);
        StopRetryCoroutine(ref gameListRetryCoroutine);
        AbortRequest(ref categoryRequest);
        AbortRequest(ref gameListRequest);
        //ClearTypeItems();
        //ClearGameItems();
        ClearCoverTextureCache();
        networkFailureKeys.Clear();
        ReleaseRuntimeQrTexture();
    }
}
