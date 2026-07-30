using MTFrame;
using MTFrame.MTEvent;
using MTPanel;

public class UIState : BaseState
{
    //注意state一定要在get里面监听事件，没有的话就写成下面样子
    //这里一般用来监听Panel切换
    public override string[] ListenerMessageID
    {
        get
        {
            return new string[]
            {
                //事件名string类型
                EventType.PanelSwitch.ToString(),
            };
        }
        set { }
    }

    public override void OnListenerMessage(EventParamete parameteData)
    {

        //接收监听事件的数据，然后用swich判断做处理

        if (parameteData.EvendName == EventType.PanelSwitch.ToString())
        {
            PanelName panelName = parameteData.GetParameter<PanelName>()[0];
            switch (panelName)
            {
                case PanelName.WaitPanel:
                    CurrentTask.ChangeTask(new WaitTask(this));
                    break;
                default:
                    break;
            }
        }
    }

    public override void Enter()
    {
        base.Enter();
        CurrentTask.ChangeTask(new WaitTask(this));
    }


    /// <summary>
    /// 切换UI Panel
    /// </summary>
    /// <param name="panelName"></param>
    public static void SwitchPanel(PanelName panelName)
    {
        EventParamete eventParamete = new EventParamete();
        eventParamete.AddParameter(panelName);
        EventManager.TriggerEvent(GenericEventEnumType.Message, EventType.PanelSwitch.ToString(), eventParamete);
    }

    public static void SendEventMsg(object o,string EventName,GenericEventEnumType genericEvent = GenericEventEnumType.Generic)
    {
        EventParamete eventParamete = new EventParamete();
        eventParamete.AddParameter(o);
        EventManager.TriggerEvent(genericEvent, EventName, eventParamete);
    }
}
