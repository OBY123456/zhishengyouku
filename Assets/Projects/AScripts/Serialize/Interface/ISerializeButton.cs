
using System.Collections.Generic;

//要和MainBehavior一起用，没出现按钮就右键编辑器，选择Refresh一下
public interface ISerializeButton
{
    /// <summary>
    /// 记录序列化按钮名字
    /// </summary>
    List<string> SerializeButtonName { get; }
    /// <summary>
    /// 记录序列化按钮回调方法
    /// </summary>
    List<System.Action> SerializeButtonMethod { get;}
}