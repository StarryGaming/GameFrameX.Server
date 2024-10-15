﻿using System.Collections.Concurrent;
using System.Text;
using GameFrameX.Extension;
using GameFrameX.NetWork.Abstractions;
using GameFrameX.NetWork.Messages;
using GameFrameX.ProtoBuf.Net;
using GameFrameX.Setting;
using GameFrameX.Utility;
using Newtonsoft.Json;

namespace GameFrameX.NetWork;

/// <summary>
/// 内部消息
/// </summary>
public class InnerNetworkMessage : IInnerNetworkMessage
{
    /// <summary>
    /// 消息ID
    /// </summary>
    public int MessageId { get; private set; }

    /// <summary>
    /// 设置消息ID
    /// </summary>
    /// <param name="messageId"></param>
    public void SetMessageId(int messageId)
    {
        MessageId = messageId;
    }

    /// <summary>
    /// 消息唯一ID
    /// </summary>
    public int UniqueId { get; private set; }


    /// <summary>
    /// 更新消息唯一ID
    /// </summary>
    public void UpdateUniqueId()
    {
        UniqueId = IdGenerator.GetNextUniqueIntId();
    }

    /// <summary>
    /// 设置消息唯一ID
    /// </summary>
    /// <param name="uniqueId"></param>
    public void SetUniqueId(int uniqueId)
    {
        UniqueId = uniqueId;
    }

    /// <summary>
    /// 消息发送打印日志
    /// </summary>
    /// <param name="srcServerType"></param>
    /// <param name="destServerType"></param>
    /// <returns></returns>
    public string ToSendMessageString(ServerType srcServerType, ServerType destServerType)
    {
        return $"---发送[{srcServerType} To {destServerType}] {ToMessageString()}";
    }

    /// <summary>
    /// 消息接收打印日志
    /// </summary>
    /// <param name="srcServerType"></param>
    /// <param name="destServerType"></param>
    /// <returns></returns>
    public string ToReceiveMessageString(ServerType srcServerType, ServerType destServerType)
    {
        return $"---收到[{srcServerType} To {destServerType}] {ToMessageString()}";
    }


    private readonly StringBuilder _stringBuilder = new StringBuilder();

    /// <summary>
    /// 消息字符串
    /// </summary>
    /// <returns></returns>
    public string ToMessageString()
    {
        _stringBuilder.Clear();
        _stringBuilder.AppendLine();
        _stringBuilder.AppendLine($"{'\u2193'.RepeatChar(100)}");
        _stringBuilder.AppendLine(
            $"\u2193---MessageType:[{MessageType?.Name.CenterAlignedText(20)}]---MessageId:[{MessageId.ToString().CenterAlignedText(10)}]---MainId:[{MessageIdUtility.GetMainId(MessageId).ToString().CenterAlignedText(5)}]---SubId:[{MessageIdUtility.GetSubId(MessageId).ToString().CenterAlignedText(5)}]---\u2193");
        _stringBuilder.AppendLine($"{ToString().WordWrap(100),-100}");
        _stringBuilder.AppendLine($"{'\u2191'.RepeatChar(100)}");
        _stringBuilder.AppendLine();
        return _stringBuilder.ToString();
    }

    /// <summary>
    /// 获取格式化后的消息字符串
    /// </summary>
    /// <returns></returns>
    public string ToFormatMessageString()
    {
        return $"消息:[{MessageId}, {UniqueId}, {GetType().Name}, {OperationType}] 消息内容:{this}";
    }

    /// <summary>
    /// 消息操作业务类型
    /// </summary>
    public MessageOperationType OperationType { get; private set; }

    /// <summary>
    /// 设置消息操作业务类型
    /// </summary>
    /// <param name="operationType"></param>
    public void SetOperationType(MessageOperationType operationType)
    {
        OperationType = operationType;
    }

    /// <summary>
    /// 消息数据
    /// </summary>
    [JsonIgnore]
    public byte[] MessageData { get; private set; }

    /// <summary>
    /// 消息数据长度
    /// </summary>
    public ushort MessageDataLength { get; private set; }

    /// <summary>
    /// 消息类型
    /// </summary>
    [JsonIgnore]
    public Type MessageType { get; private set; }

    /// <summary>
    /// 设置消息类型
    /// </summary>
    /// <param name="messageType"></param>
    public void SetMessageType(Type messageType)
    {
        MessageType = messageType;
    }

    /// <summary>
    /// 反序列化消息内容
    /// </summary>
    /// <returns></returns>
    public INetworkMessage DeserializeMessageObject()
    {
        var value = ProtoBufSerializerHelper.Deserialize(MessageData, MessageType);
        return (MessageObject)value;
    }

    /// <summary>
    /// 设置消息内容
    /// </summary>
    /// <param name="messageData"></param>
    public void SetMessageData(byte[] messageData)
    {
        MessageData = messageData;
        MessageDataLength = (ushort)messageData.Length;
    }

    /// <summary>
    /// 消息转字符串
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return JsonHelper.Serialize(this);
    }

    /// <summary>
    /// 创建消息
    /// </summary>
    /// <param name="message"></param>
    /// <param name="operationType"></param>
    /// <returns></returns>
    public static InnerNetworkMessage Create(IOuterMessage message, MessageOperationType operationType)
    {
        var innerMessage = new InnerNetworkMessage();
        innerMessage.SetOperationType(operationType);
        innerMessage.SetMessageType(message.MessageType);
        innerMessage.SetMessageData(message.MessageData);
        innerMessage.SetMessageId(message.MessageId);
        innerMessage.SetData(GlobalConst.UniqueIdIdKey, message.UniqueId);
        return innerMessage;
    }

    /// <summary>
    /// 创建消息
    /// </summary>
    /// <param name="message"></param>
    /// <param name="header"></param>
    /// <param name="operationType"></param>
    /// <returns></returns>
    public static InnerNetworkMessage Create(INetworkMessage message, MessageObjectHeader header, MessageOperationType operationType)
    {
        var innerMessage = new InnerNetworkMessage();
        innerMessage.SetOperationType(operationType);
        innerMessage.SetMessageType(message.GetType());
        innerMessage.SetUniqueId(message.UniqueId);
        var buffer = ProtoBufSerializerHelper.Serialize(message);
        innerMessage.SetMessageData(buffer);
        innerMessage.SetMessageHeader(header);
        innerMessage.SetMessageId(message.MessageId);
        innerMessage.SetData(GlobalConst.UniqueIdIdKey, message.UniqueId);
        return innerMessage;
    }

    /// <summary>
    /// 消息头
    /// </summary>
    public MessageObjectHeader Header { get; private set; }

    /// <summary>
    /// 设置消息头
    /// </summary>
    /// <param name="header"></param>
    public void SetMessageHeader(MessageObjectHeader header)
    {
        Header = header;
    }

    private readonly ConcurrentDictionary<string, object> _data = new ConcurrentDictionary<string, object>();

    /// <summary>
    /// 获取自定义数据
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, object> GetData()
    {
        return _data.ToDictionary();
    }

    /// <summary>
    /// 设置自定义数据
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SetData(string key, object value)
    {
        _data[key] = value;
    }

    /// <summary>
    /// 获取自定义数据
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public object GetData(string key)
    {
        _data.TryGetValue(key, out var value);
        return value;
    }

    /// <summary>
    /// 删除自定义数据
    /// </summary>
    /// <param name="key"></param>
    public bool RemoveData(string key)
    {
        return _data.Remove(key, out _);
    }

    /// <summary>
    /// 清除自定义数据
    /// </summary>
    public void ClearData()
    {
        _data.Clear();
    }
}