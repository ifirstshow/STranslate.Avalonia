using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace OcrProtobuf;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class OcrResponse : IMessage<OcrResponse>, IMessage, IEquatable<OcrResponse>, IDeepCloneable<OcrResponse>, IBufferMessage
{
	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static class Types
	{
		[DebuggerDisplay("{ToString(),nq}")]
		public sealed class OcrResult : IMessage<OcrResult>, IMessage, IEquatable<OcrResult>, IDeepCloneable<OcrResult>, IBufferMessage
		{
			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public static class Types
			{
				[DebuggerDisplay("{ToString(),nq}")]
				public sealed class ResultPos : IMessage<ResultPos>, IMessage, IEquatable<ResultPos>, IDeepCloneable<ResultPos>, IBufferMessage
				{
					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public static class Types
					{
						[DebuggerDisplay("{ToString(),nq}")]
						public sealed class PosXY : IMessage<PosXY>, IMessage, IEquatable<PosXY>, IDeepCloneable<PosXY>, IBufferMessage
						{
							private static readonly MessageParser<PosXY> _parser = new MessageParser<PosXY>((Func<PosXY>)(() => new PosXY()));

							private UnknownFieldSet _unknownFields;

							public const int XFieldNumber = 1;

							private float x_;

							public const int YFieldNumber = 2;

							private float y_;

							[DebuggerNonUserCode]
							[GeneratedCode("protoc", null)]
							public static MessageParser<PosXY> Parser => _parser;

							[DebuggerNonUserCode]
							[GeneratedCode("protoc", null)]
							public static MessageDescriptor Descriptor => ResultPos.Descriptor.NestedTypes[0];

							[DebuggerNonUserCode]
							[GeneratedCode("protoc", null)]
							MessageDescriptor IMessage.Descriptor => Descriptor;

							[DebuggerNonUserCode]
							[GeneratedCode("protoc", null)]
							public float X
							{
								get
								{
									return x_;
								}
								set
								{
									x_ = value;
								}
							}

							[DebuggerNonUserCode]
							[GeneratedCode("protoc", null)]
							public float Y
							{
								get
								{
									return y_;
								}
								set
								{
									y_ = value;
								}
							}

							[DebuggerNonUserCode]
							[GeneratedCode("protoc", null)]
							public PosXY()
							{
							}

							[DebuggerNonUserCode]
							[GeneratedCode("protoc", null)]
							public PosXY(PosXY other)
								: this()
							{
								x_ = other.x_;
								y_ = other.y_;
								_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
							}

							[DebuggerNonUserCode]
							[GeneratedCode("protoc", null)]
							public PosXY Clone()
							{
								return new PosXY(this);
							}

							[DebuggerNonUserCode]
							[GeneratedCode("protoc", null)]
							public override bool Equals(object other)
							{
								return Equals(other as PosXY);
							}

							[DebuggerNonUserCode]
							[GeneratedCode("protoc", null)]
							public bool Equals(PosXY other)
							{
								if (other == null)
								{
									return false;
								}
								if (other == this)
								{
									return true;
								}
								if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(X, other.X))
								{
									return false;
								}
								if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Y, other.Y))
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
								if (X != 0f)
								{
									num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(X);
								}
								if (Y != 0f)
								{
									num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Y);
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
								if (X != 0f)
								{
									output.WriteRawTag((byte)13);
									output.WriteFloat(X);
								}
								if (Y != 0f)
								{
									output.WriteRawTag((byte)21);
									output.WriteFloat(Y);
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
								if (X != 0f)
								{
									num += 5;
								}
								if (Y != 0f)
								{
									num += 5;
								}
								if (_unknownFields != null)
								{
									num += _unknownFields.CalculateSize();
								}
								return num;
							}

							[DebuggerNonUserCode]
							[GeneratedCode("protoc", null)]
							public void MergeFrom(PosXY other)
							{
								if (other != null)
								{
									if (other.X != 0f)
									{
										X = other.X;
									}
									if (other.Y != 0f)
									{
										Y = other.Y;
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
									case 13u:
										X = input.ReadFloat();
										break;
									case 21u:
										Y = input.ReadFloat();
										break;
									}
								}
							}
						}
					}

					private static readonly MessageParser<ResultPos> _parser = new MessageParser<ResultPos>((Func<ResultPos>)(() => new ResultPos()));

					private UnknownFieldSet _unknownFields;

					public const int PosFieldNumber = 1;

					private static readonly FieldCodec<Types.PosXY> _repeated_pos_codec = FieldCodec.ForMessage<Types.PosXY>(10u, Types.PosXY.Parser);

					private readonly RepeatedField<Types.PosXY> pos_ = new RepeatedField<Types.PosXY>();

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public static MessageParser<ResultPos> Parser => _parser;

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public static MessageDescriptor Descriptor => OcrResult.Descriptor.NestedTypes[0];

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					MessageDescriptor IMessage.Descriptor => Descriptor;

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public RepeatedField<Types.PosXY> Pos => pos_;

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public ResultPos()
					{
					}

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public ResultPos(ResultPos other)
						: this()
					{
						pos_ = other.pos_.Clone();
						_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
					}

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public ResultPos Clone()
					{
						return new ResultPos(this);
					}

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public override bool Equals(object other)
					{
						return Equals(other as ResultPos);
					}

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public bool Equals(ResultPos other)
					{
						if (other == null)
						{
							return false;
						}
						if (other == this)
						{
							return true;
						}
						if (!pos_.Equals(other.pos_))
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
						num ^= ((object)pos_).GetHashCode();
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
						pos_.WriteTo(ref output, _repeated_pos_codec);
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
						num += pos_.CalculateSize(_repeated_pos_codec);
						if (_unknownFields != null)
						{
							num += _unknownFields.CalculateSize();
						}
						return num;
					}

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public void MergeFrom(ResultPos other)
					{
						if (other != null)
						{
							pos_.Add((IEnumerable<Types.PosXY>)other.pos_);
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
							if (num != 10)
							{
								_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
							}
							else
							{
								pos_.AddEntriesFrom(ref input, _repeated_pos_codec);
							}
						}
					}
				}

				[DebuggerDisplay("{ToString(),nq}")]
				public sealed class SingleResult : IMessage<SingleResult>, IMessage, IEquatable<SingleResult>, IDeepCloneable<SingleResult>, IBufferMessage
				{
					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public static class Types
					{
						[DebuggerDisplay("{ToString(),nq}")]
						public sealed class OneResult : IMessage<OneResult>, IMessage, IEquatable<OneResult>, IDeepCloneable<OneResult>, IBufferMessage
						{
							private static readonly MessageParser<OneResult> _parser = new MessageParser<OneResult>((Func<OneResult>)(() => new OneResult()));

							private UnknownFieldSet _unknownFields;

							public const int OnePosFieldNumber = 1;

							private ResultPos onePos_;

							public const int OneStrUtf8FieldNumber = 2;

							private ByteString oneStrUtf8_ = ByteString.Empty;

							[DebuggerNonUserCode]
							[GeneratedCode("protoc", null)]
							public static MessageParser<OneResult> Parser => _parser;

							[DebuggerNonUserCode]
							[GeneratedCode("protoc", null)]
							public static MessageDescriptor Descriptor => SingleResult.Descriptor.NestedTypes[0];

							[DebuggerNonUserCode]
							[GeneratedCode("protoc", null)]
							MessageDescriptor IMessage.Descriptor => Descriptor;

							[DebuggerNonUserCode]
							[GeneratedCode("protoc", null)]
							public ResultPos OnePos
							{
								get
								{
									return onePos_;
								}
								set
								{
									onePos_ = value;
								}
							}

							[DebuggerNonUserCode]
							[GeneratedCode("protoc", null)]
							public ByteString OneStrUtf8
							{
								get
								{
									return oneStrUtf8_;
								}
								set
								{
									oneStrUtf8_ = ProtoPreconditions.CheckNotNull<ByteString>(value, "value");
								}
							}

							[DebuggerNonUserCode]
							[GeneratedCode("protoc", null)]
							public OneResult()
							{
							}

							[DebuggerNonUserCode]
							[GeneratedCode("protoc", null)]
							public OneResult(OneResult other)
								: this()
							{
								onePos_ = ((other.onePos_ != null) ? other.onePos_.Clone() : null);
								oneStrUtf8_ = other.oneStrUtf8_;
								_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
							}

							[DebuggerNonUserCode]
							[GeneratedCode("protoc", null)]
							public OneResult Clone()
							{
								return new OneResult(this);
							}

							[DebuggerNonUserCode]
							[GeneratedCode("protoc", null)]
							public override bool Equals(object other)
							{
								return Equals(other as OneResult);
							}

							[DebuggerNonUserCode]
							[GeneratedCode("protoc", null)]
							public bool Equals(OneResult other)
							{
								if (other == null)
								{
									return false;
								}
								if (other == this)
								{
									return true;
								}
								if (!object.Equals(OnePos, other.OnePos))
								{
									return false;
								}
								if (OneStrUtf8 != other.OneStrUtf8)
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
								if (onePos_ != null)
								{
									num ^= OnePos.GetHashCode();
								}
								if (OneStrUtf8.Length != 0)
								{
									num ^= ((object)OneStrUtf8).GetHashCode();
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
								if (onePos_ != null)
								{
									output.WriteRawTag((byte)10);
									output.WriteMessage((IMessage)(object)OnePos);
								}
								if (OneStrUtf8.Length != 0)
								{
									output.WriteRawTag((byte)18);
									output.WriteBytes(OneStrUtf8);
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
								if (onePos_ != null)
								{
									num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)OnePos);
								}
								if (OneStrUtf8.Length != 0)
								{
									num += 1 + CodedOutputStream.ComputeBytesSize(OneStrUtf8);
								}
								if (_unknownFields != null)
								{
									num += _unknownFields.CalculateSize();
								}
								return num;
							}

							[DebuggerNonUserCode]
							[GeneratedCode("protoc", null)]
							public void MergeFrom(OneResult other)
							{
								if (other == null)
								{
									return;
								}
								if (other.onePos_ != null)
								{
									if (onePos_ == null)
									{
										OnePos = new ResultPos();
									}
									OnePos.MergeFrom(other.OnePos);
								}
								if (other.OneStrUtf8.Length != 0)
								{
									OneStrUtf8 = other.OneStrUtf8;
								}
								_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
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
									case 10u:
										if (onePos_ == null)
										{
											OnePos = new ResultPos();
										}
										input.ReadMessage((IMessage)(object)OnePos);
										break;
									case 18u:
										OneStrUtf8 = input.ReadBytes();
										break;
									}
								}
							}
						}
					}

					private static readonly MessageParser<SingleResult> _parser = new MessageParser<SingleResult>((Func<SingleResult>)(() => new SingleResult()));

					private UnknownFieldSet _unknownFields;

					public const int SinglePosFieldNumber = 1;

					private ResultPos singlePos_;

					public const int SingleStrUtf8FieldNumber = 2;

					private ByteString singleStrUtf8_ = ByteString.Empty;

					public const int SingleRateFieldNumber = 3;

					private float singleRate_;

					public const int OneResultFieldNumber = 4;

					private static readonly FieldCodec<Types.OneResult> _repeated_oneResult_codec = FieldCodec.ForMessage<Types.OneResult>(34u, Types.OneResult.Parser);

					private readonly RepeatedField<Types.OneResult> oneResult_ = new RepeatedField<Types.OneResult>();

					public const int LeftFieldNumber = 5;

					private float left_;

					public const int TopFieldNumber = 6;

					private float top_;

					public const int RightFieldNumber = 7;

					private float right_;

					public const int BottomFieldNumber = 8;

					private float bottom_;

					public const int Unknown0FieldNumber = 9;

					private int unknown0_;

					public const int UnknownPosFieldNumber = 10;

					private ResultPos unknownPos_;

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public static MessageParser<SingleResult> Parser => _parser;

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public static MessageDescriptor Descriptor => OcrResult.Descriptor.NestedTypes[1];

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					MessageDescriptor IMessage.Descriptor => Descriptor;

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public ResultPos SinglePos
					{
						get
						{
							return singlePos_;
						}
						set
						{
							singlePos_ = value;
						}
					}

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public ByteString SingleStrUtf8
					{
						get
						{
							return singleStrUtf8_;
						}
						set
						{
							singleStrUtf8_ = ProtoPreconditions.CheckNotNull<ByteString>(value, "value");
						}
					}

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public float SingleRate
					{
						get
						{
							return singleRate_;
						}
						set
						{
							singleRate_ = value;
						}
					}

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public RepeatedField<Types.OneResult> OneResult => oneResult_;

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public float Left
					{
						get
						{
							return left_;
						}
						set
						{
							left_ = value;
						}
					}

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public float Top
					{
						get
						{
							return top_;
						}
						set
						{
							top_ = value;
						}
					}

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public float Right
					{
						get
						{
							return right_;
						}
						set
						{
							right_ = value;
						}
					}

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public float Bottom
					{
						get
						{
							return bottom_;
						}
						set
						{
							bottom_ = value;
						}
					}

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public int Unknown0
					{
						get
						{
							return unknown0_;
						}
						set
						{
							unknown0_ = value;
						}
					}

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public ResultPos UnknownPos
					{
						get
						{
							return unknownPos_;
						}
						set
						{
							unknownPos_ = value;
						}
					}

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public SingleResult()
					{
					}

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public SingleResult(SingleResult other)
						: this()
					{
						singlePos_ = ((other.singlePos_ != null) ? other.singlePos_.Clone() : null);
						singleStrUtf8_ = other.singleStrUtf8_;
						singleRate_ = other.singleRate_;
						oneResult_ = other.oneResult_.Clone();
						left_ = other.left_;
						top_ = other.top_;
						right_ = other.right_;
						bottom_ = other.bottom_;
						unknown0_ = other.unknown0_;
						unknownPos_ = ((other.unknownPos_ != null) ? other.unknownPos_.Clone() : null);
						_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
					}

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public SingleResult Clone()
					{
						return new SingleResult(this);
					}

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public override bool Equals(object other)
					{
						return Equals(other as SingleResult);
					}

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public bool Equals(SingleResult other)
					{
						if (other == null)
						{
							return false;
						}
						if (other == this)
						{
							return true;
						}
						if (!object.Equals(SinglePos, other.SinglePos))
						{
							return false;
						}
						if (SingleStrUtf8 != other.SingleStrUtf8)
						{
							return false;
						}
						if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(SingleRate, other.SingleRate))
						{
							return false;
						}
						if (!oneResult_.Equals(other.oneResult_))
						{
							return false;
						}
						if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Left, other.Left))
						{
							return false;
						}
						if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Top, other.Top))
						{
							return false;
						}
						if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Right, other.Right))
						{
							return false;
						}
						if (!ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Bottom, other.Bottom))
						{
							return false;
						}
						if (Unknown0 != other.Unknown0)
						{
							return false;
						}
						if (!object.Equals(UnknownPos, other.UnknownPos))
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
						if (singlePos_ != null)
						{
							num ^= SinglePos.GetHashCode();
						}
						if (SingleStrUtf8.Length != 0)
						{
							num ^= ((object)SingleStrUtf8).GetHashCode();
						}
						if (SingleRate != 0f)
						{
							num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(SingleRate);
						}
						num ^= ((object)oneResult_).GetHashCode();
						if (Left != 0f)
						{
							num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Left);
						}
						if (Top != 0f)
						{
							num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Top);
						}
						if (Right != 0f)
						{
							num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Right);
						}
						if (Bottom != 0f)
						{
							num ^= ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Bottom);
						}
						if (Unknown0 != 0)
						{
							num ^= Unknown0.GetHashCode();
						}
						if (unknownPos_ != null)
						{
							num ^= UnknownPos.GetHashCode();
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
						if (singlePos_ != null)
						{
							output.WriteRawTag((byte)10);
							output.WriteMessage((IMessage)(object)SinglePos);
						}
						if (SingleStrUtf8.Length != 0)
						{
							output.WriteRawTag((byte)18);
							output.WriteBytes(SingleStrUtf8);
						}
						if (SingleRate != 0f)
						{
							output.WriteRawTag((byte)29);
							output.WriteFloat(SingleRate);
						}
						oneResult_.WriteTo(ref output, _repeated_oneResult_codec);
						if (Left != 0f)
						{
							output.WriteRawTag((byte)45);
							output.WriteFloat(Left);
						}
						if (Top != 0f)
						{
							output.WriteRawTag((byte)53);
							output.WriteFloat(Top);
						}
						if (Right != 0f)
						{
							output.WriteRawTag((byte)61);
							output.WriteFloat(Right);
						}
						if (Bottom != 0f)
						{
							output.WriteRawTag((byte)69);
							output.WriteFloat(Bottom);
						}
						if (Unknown0 != 0)
						{
							output.WriteRawTag((byte)72);
							output.WriteInt32(Unknown0);
						}
						if (unknownPos_ != null)
						{
							output.WriteRawTag((byte)82);
							output.WriteMessage((IMessage)(object)UnknownPos);
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
						if (singlePos_ != null)
						{
							num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)SinglePos);
						}
						if (SingleStrUtf8.Length != 0)
						{
							num += 1 + CodedOutputStream.ComputeBytesSize(SingleStrUtf8);
						}
						if (SingleRate != 0f)
						{
							num += 5;
						}
						num += oneResult_.CalculateSize(_repeated_oneResult_codec);
						if (Left != 0f)
						{
							num += 5;
						}
						if (Top != 0f)
						{
							num += 5;
						}
						if (Right != 0f)
						{
							num += 5;
						}
						if (Bottom != 0f)
						{
							num += 5;
						}
						if (Unknown0 != 0)
						{
							num += 1 + CodedOutputStream.ComputeInt32Size(Unknown0);
						}
						if (unknownPos_ != null)
						{
							num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)UnknownPos);
						}
						if (_unknownFields != null)
						{
							num += _unknownFields.CalculateSize();
						}
						return num;
					}

					[DebuggerNonUserCode]
					[GeneratedCode("protoc", null)]
					public void MergeFrom(SingleResult other)
					{
						if (other == null)
						{
							return;
						}
						if (other.singlePos_ != null)
						{
							if (singlePos_ == null)
							{
								SinglePos = new ResultPos();
							}
							SinglePos.MergeFrom(other.SinglePos);
						}
						if (other.SingleStrUtf8.Length != 0)
						{
							SingleStrUtf8 = other.SingleStrUtf8;
						}
						if (other.SingleRate != 0f)
						{
							SingleRate = other.SingleRate;
						}
						oneResult_.Add((IEnumerable<Types.OneResult>)other.oneResult_);
						if (other.Left != 0f)
						{
							Left = other.Left;
						}
						if (other.Top != 0f)
						{
							Top = other.Top;
						}
						if (other.Right != 0f)
						{
							Right = other.Right;
						}
						if (other.Bottom != 0f)
						{
							Bottom = other.Bottom;
						}
						if (other.Unknown0 != 0)
						{
							Unknown0 = other.Unknown0;
						}
						if (other.unknownPos_ != null)
						{
							if (unknownPos_ == null)
							{
								UnknownPos = new ResultPos();
							}
							UnknownPos.MergeFrom(other.UnknownPos);
						}
						_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
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
							case 10u:
								if (singlePos_ == null)
								{
									SinglePos = new ResultPos();
								}
								input.ReadMessage((IMessage)(object)SinglePos);
								break;
							case 18u:
								SingleStrUtf8 = input.ReadBytes();
								break;
							case 29u:
								SingleRate = input.ReadFloat();
								break;
							case 34u:
								oneResult_.AddEntriesFrom(ref input, _repeated_oneResult_codec);
								break;
							case 45u:
								Left = input.ReadFloat();
								break;
							case 53u:
								Top = input.ReadFloat();
								break;
							case 61u:
								Right = input.ReadFloat();
								break;
							case 69u:
								Bottom = input.ReadFloat();
								break;
							case 72u:
								Unknown0 = input.ReadInt32();
								break;
							case 82u:
								if (unknownPos_ == null)
								{
									UnknownPos = new ResultPos();
								}
								input.ReadMessage((IMessage)(object)UnknownPos);
								break;
							}
						}
					}
				}
			}

			private static readonly MessageParser<OcrResult> _parser = new MessageParser<OcrResult>((Func<OcrResult>)(() => new OcrResult()));

			private UnknownFieldSet _unknownFields;

			public const int SingleResultFieldNumber = 1;

			private static readonly FieldCodec<Types.SingleResult> _repeated_singleResult_codec = FieldCodec.ForMessage<Types.SingleResult>(10u, Types.SingleResult.Parser);

			private readonly RepeatedField<Types.SingleResult> singleResult_ = new RepeatedField<Types.SingleResult>();

			public const int Unknown1FieldNumber = 2;

			private int unknown1_;

			public const int Unknown2FieldNumber = 3;

			private int unknown2_;

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public static MessageParser<OcrResult> Parser => _parser;

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public static MessageDescriptor Descriptor => OcrResponse.Descriptor.NestedTypes[0];

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			MessageDescriptor IMessage.Descriptor => Descriptor;

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public RepeatedField<Types.SingleResult> SingleResult => singleResult_;

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public int Unknown1
			{
				get
				{
					return unknown1_;
				}
				set
				{
					unknown1_ = value;
				}
			}

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public int Unknown2
			{
				get
				{
					return unknown2_;
				}
				set
				{
					unknown2_ = value;
				}
			}

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public OcrResult()
			{
			}

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public OcrResult(OcrResult other)
				: this()
			{
				singleResult_ = other.singleResult_.Clone();
				unknown1_ = other.unknown1_;
				unknown2_ = other.unknown2_;
				_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
			}

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public OcrResult Clone()
			{
				return new OcrResult(this);
			}

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public override bool Equals(object other)
			{
				return Equals(other as OcrResult);
			}

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public bool Equals(OcrResult other)
			{
				if (other == null)
				{
					return false;
				}
				if (other == this)
				{
					return true;
				}
				if (!singleResult_.Equals(other.singleResult_))
				{
					return false;
				}
				if (Unknown1 != other.Unknown1)
				{
					return false;
				}
				if (Unknown2 != other.Unknown2)
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
				num ^= ((object)singleResult_).GetHashCode();
				if (Unknown1 != 0)
				{
					num ^= Unknown1.GetHashCode();
				}
				if (Unknown2 != 0)
				{
					num ^= Unknown2.GetHashCode();
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
				singleResult_.WriteTo(ref output, _repeated_singleResult_codec);
				if (Unknown1 != 0)
				{
					output.WriteRawTag((byte)16);
					output.WriteInt32(Unknown1);
				}
				if (Unknown2 != 0)
				{
					output.WriteRawTag((byte)24);
					output.WriteInt32(Unknown2);
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
				num += singleResult_.CalculateSize(_repeated_singleResult_codec);
				if (Unknown1 != 0)
				{
					num += 1 + CodedOutputStream.ComputeInt32Size(Unknown1);
				}
				if (Unknown2 != 0)
				{
					num += 1 + CodedOutputStream.ComputeInt32Size(Unknown2);
				}
				if (_unknownFields != null)
				{
					num += _unknownFields.CalculateSize();
				}
				return num;
			}

			[DebuggerNonUserCode]
			[GeneratedCode("protoc", null)]
			public void MergeFrom(OcrResult other)
			{
				if (other != null)
				{
					singleResult_.Add((IEnumerable<Types.SingleResult>)other.singleResult_);
					if (other.Unknown1 != 0)
					{
						Unknown1 = other.Unknown1;
					}
					if (other.Unknown2 != 0)
					{
						Unknown2 = other.Unknown2;
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
					case 10u:
						singleResult_.AddEntriesFrom(ref input, _repeated_singleResult_codec);
						break;
					case 16u:
						Unknown1 = input.ReadInt32();
						break;
					case 24u:
						Unknown2 = input.ReadInt32();
						break;
					}
				}
			}
		}
	}

	private static readonly MessageParser<OcrResponse> _parser = new MessageParser<OcrResponse>((Func<OcrResponse>)(() => new OcrResponse()));

	private UnknownFieldSet _unknownFields;

	public const int TypeFieldNumber = 1;

	private int type_;

	public const int TaskIdFieldNumber = 2;

	private int taskId_;

	public const int ErrCodeFieldNumber = 3;

	private int errCode_;

	public const int OcrResultFieldNumber = 4;

	private Types.OcrResult ocrResult_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<OcrResponse> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => OcrProtobufReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

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
	public int TaskId
	{
		get
		{
			return taskId_;
		}
		set
		{
			taskId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int ErrCode
	{
		get
		{
			return errCode_;
		}
		set
		{
			errCode_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public Types.OcrResult OcrResult
	{
		get
		{
			return ocrResult_;
		}
		set
		{
			ocrResult_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OcrResponse()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OcrResponse(OcrResponse other)
		: this()
	{
		type_ = other.type_;
		taskId_ = other.taskId_;
		errCode_ = other.errCode_;
		ocrResult_ = ((other.ocrResult_ != null) ? other.ocrResult_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public OcrResponse Clone()
	{
		return new OcrResponse(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as OcrResponse);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(OcrResponse other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Type != other.Type)
		{
			return false;
		}
		if (TaskId != other.TaskId)
		{
			return false;
		}
		if (ErrCode != other.ErrCode)
		{
			return false;
		}
		if (!object.Equals(OcrResult, other.OcrResult))
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
		if (Type != 0)
		{
			num ^= Type.GetHashCode();
		}
		if (TaskId != 0)
		{
			num ^= TaskId.GetHashCode();
		}
		if (ErrCode != 0)
		{
			num ^= ErrCode.GetHashCode();
		}
		if (ocrResult_ != null)
		{
			num ^= OcrResult.GetHashCode();
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
		if (Type != 0)
		{
			output.WriteRawTag((byte)8);
			output.WriteInt32(Type);
		}
		if (TaskId != 0)
		{
			output.WriteRawTag((byte)16);
			output.WriteInt32(TaskId);
		}
		if (ErrCode != 0)
		{
			output.WriteRawTag((byte)24);
			output.WriteInt32(ErrCode);
		}
		if (ocrResult_ != null)
		{
			output.WriteRawTag((byte)34);
			output.WriteMessage((IMessage)(object)OcrResult);
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
		if (Type != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Type);
		}
		if (TaskId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(TaskId);
		}
		if (ErrCode != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(ErrCode);
		}
		if (ocrResult_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)OcrResult);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(OcrResponse other)
	{
		if (other == null)
		{
			return;
		}
		if (other.Type != 0)
		{
			Type = other.Type;
		}
		if (other.TaskId != 0)
		{
			TaskId = other.TaskId;
		}
		if (other.ErrCode != 0)
		{
			ErrCode = other.ErrCode;
		}
		if (other.ocrResult_ != null)
		{
			if (ocrResult_ == null)
			{
				OcrResult = new Types.OcrResult();
			}
			OcrResult.MergeFrom(other.OcrResult);
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
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
				Type = input.ReadInt32();
				break;
			case 16u:
				TaskId = input.ReadInt32();
				break;
			case 24u:
				ErrCode = input.ReadInt32();
				break;
			case 34u:
				if (ocrResult_ == null)
				{
					OcrResult = new Types.OcrResult();
				}
				input.ReadMessage((IMessage)(object)OcrResult);
				break;
			}
		}
	}
}
