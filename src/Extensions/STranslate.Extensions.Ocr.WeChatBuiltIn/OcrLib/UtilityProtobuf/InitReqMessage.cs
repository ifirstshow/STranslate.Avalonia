using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace UtilityProtobuf;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class InitReqMessage : IMessage<InitReqMessage>, IMessage, IEquatable<InitReqMessage>, IDeepCloneable<InitReqMessage>, IBufferMessage
{
	private static readonly MessageParser<InitReqMessage> _parser = new MessageParser<InitReqMessage>((Func<InitReqMessage>)(() => new InitReqMessage()));

	private UnknownFieldSet _unknownFields;

	public const int ExpireTimestampFieldNumber = 1;

	private int expireTimestamp_;

	public const int TypeFieldNumber = 2;

	private int type_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<InitReqMessage> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => UtilityProtobufReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ExpireTimestamp
	{
		get
		{
			return expireTimestamp_;
		}
		set
		{
			expireTimestamp_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Type
	{
		get
		{
			return type_;
		}
		set
		{
			type_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public InitReqMessage()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public InitReqMessage(InitReqMessage other)
		: this()
	{
		expireTimestamp_ = other.expireTimestamp_;
		type_ = other.type_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public InitReqMessage Clone()
	{
		return new InitReqMessage(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as InitReqMessage);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(InitReqMessage other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ExpireTimestamp != other.ExpireTimestamp)
		{
			return false;
		}
		if (Type != other.Type)
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override int GetHashCode()
	{
		int num = 1;
		if (ExpireTimestamp != 0)
		{
			num ^= ExpireTimestamp.GetHashCode();
		}
		if (Type != 0)
		{
			num ^= Type.GetHashCode();
		}
		if (_unknownFields != null)
		{
			num ^= ((object)_unknownFields).GetHashCode();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override string ToString()
	{
		return JsonFormatter.ToDiagnosticString((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void WriteTo(CodedOutputStream output)
	{
		output.WriteRawMessage((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	void IBufferMessage.InternalWriteTo(ref WriteContext output)
	{
		if (ExpireTimestamp != 0)
		{
			output.WriteRawTag((byte)8);
			output.WriteInt32(ExpireTimestamp);
		}
		if (Type != 0)
		{
			output.WriteRawTag((byte)16);
			output.WriteInt32(Type);
		}
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CalculateSize()
	{
		int num = 0;
		if (ExpireTimestamp != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ExpireTimestamp);
		}
		if (Type != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Type);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(InitReqMessage other)
	{
		if (other != null)
		{
			if (other.ExpireTimestamp != 0)
			{
				ExpireTimestamp = other.ExpireTimestamp;
			}
			if (other.Type != 0)
			{
				Type = other.Type;
			}
			_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CodedInputStream input)
	{
		input.ReadRawMessage((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	void IBufferMessage.InternalMergeFrom(ref ParseContext input)
	{
		uint num;
		while ((num = input.ReadTag()) != 0)
		{
			switch (num)
			{
			default:
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
				break;
			case 8u:
				ExpireTimestamp = input.ReadInt32();
				break;
			case 16u:
				Type = input.ReadInt32();
				break;
			}
		}
	}
}
