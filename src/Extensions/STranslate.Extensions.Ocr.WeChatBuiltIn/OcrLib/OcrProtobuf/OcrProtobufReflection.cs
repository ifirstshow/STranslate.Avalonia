using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace OcrProtobuf;

public static class OcrProtobufReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static OcrProtobufReflection()
	{
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Expected Obj, but got Unknown
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Expected Obj, but got Unknown
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Expected Obj, but got Unknown
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Expected Obj, but got Unknown
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Expected Obj, but got Unknown
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Expected Obj, but got Unknown
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Expected Obj, but got Unknown
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Expected Obj, but got Unknown
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Expected Obj, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chhwcm90by9vY3JfcHJvdG9idWYucHJvdG8SDG9jcl9wcm90b2J1ZiKoBgoL" + "T2NyUmVzcG9uc2USDAoEdHlwZRgBIAEoBRIPCgd0YXNrX2lkGAIgASgFEhAK" + "CGVycl9jb2RlGAMgASgFEjcKCm9jcl9yZXN1bHQYBCABKAsyIy5vY3JfcHJv" + "dG9idWYuT2NyUmVzcG9uc2UuT2NyUmVzdWx0Gq4FCglPY3JSZXN1bHQSRwoN" + "c2luZ2xlX3Jlc3VsdBgBIAMoCzIwLm9jcl9wcm90b2J1Zi5PY3JSZXNwb25z" + "ZS5PY3JSZXN1bHQuU2luZ2xlUmVzdWx0EhEKCXVua25vd25fMRgCIAEoBRIR" + "Cgl1bmtub3duXzIYAyABKAUabAoJUmVzdWx0UG9zEkAKA3BvcxgBIAMoCzIz" + "Lm9jcl9wcm90b2J1Zi5PY3JSZXNwb25zZS5PY3JSZXN1bHQuUmVzdWx0UG9z" + "LlBvc1hZGh0KBVBvc1hZEgkKAXgYASABKAISCQoBeRgCIAEoAhrDAwoMU2lu" + "Z2xlUmVzdWx0EkEKCnNpbmdsZV9wb3MYASABKAsyLS5vY3JfcHJvdG9idWYu" + "T2NyUmVzcG9uc2UuT2NyUmVzdWx0LlJlc3VsdFBvcxIXCg9zaW5nbGVfc3Ry" + "X3V0ZjgYAiABKAwSEwoLc2luZ2xlX3JhdGUYAyABKAISTgoKb25lX3Jlc3Vs" + "dBgEIAMoCzI6Lm9jcl9wcm90b2J1Zi5PY3JSZXNwb25zZS5PY3JSZXN1bHQu" + "U2luZ2xlUmVzdWx0Lk9uZVJlc3VsdBIMCgRsZWZ0GAUgASgCEgsKA3RvcBgG" + "IAEoAhINCgVyaWdodBgHIAEoAhIOCgZib3R0b20YCCABKAISEQoJdW5rbm93" + "bl8wGAkgASgFEkIKC3Vua25vd25fcG9zGAogASgLMi0ub2NyX3Byb3RvYnVm" + "Lk9jclJlc3BvbnNlLk9jclJlc3VsdC5SZXN1bHRQb3MaYQoJT25lUmVzdWx0" + "Ej4KB29uZV9wb3MYASABKAsyLS5vY3JfcHJvdG9idWYuT2NyUmVzcG9uc2Uu" + "T2NyUmVzdWx0LlJlc3VsdFBvcxIUCgxvbmVfc3RyX3V0ZjgYAiABKAwigAEK" + "Ck9jclJlcXVlc3QSDgoGdW5rbm93GAEgASgFEg8KB3Rhc2tfaWQYAiABKAUS" + "MwoIcGljX3BhdGgYAyABKAsyIS5vY3JfcHJvdG9idWYuT2NyUmVxdWVzdC5Q" + "aWNQYXRocxocCghQaWNQYXRocxIQCghwaWNfcGF0aBgBIAMoCWIGcHJvdG8z"), new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, new GeneratedClrTypeInfo[2]
		{
			new GeneratedClrTypeInfo(typeof(OcrResponse), (MessageParser)(object)OcrResponse.Parser, new string[4] { "Type", "TaskId", "ErrCode", "OcrResult" }, (string[])null, (Type[])null, (Extension[])null, new GeneratedClrTypeInfo[1]
			{
				new GeneratedClrTypeInfo(typeof(OcrResponse.Types.OcrResult), (MessageParser)(object)OcrResponse.Types.OcrResult.Parser, new string[3] { "SingleResult", "Unknown1", "Unknown2" }, (string[])null, (Type[])null, (Extension[])null, new GeneratedClrTypeInfo[2]
				{
					new GeneratedClrTypeInfo(typeof(OcrResponse.Types.OcrResult.Types.ResultPos), (MessageParser)(object)OcrResponse.Types.OcrResult.Types.ResultPos.Parser, new string[1] { "Pos" }, (string[])null, (Type[])null, (Extension[])null, new GeneratedClrTypeInfo[1]
					{
						new GeneratedClrTypeInfo(typeof(OcrResponse.Types.OcrResult.Types.ResultPos.Types.PosXY), (MessageParser)(object)OcrResponse.Types.OcrResult.Types.ResultPos.Types.PosXY.Parser, new string[2] { "X", "Y" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
					}),
					new GeneratedClrTypeInfo(typeof(OcrResponse.Types.OcrResult.Types.SingleResult), (MessageParser)(object)OcrResponse.Types.OcrResult.Types.SingleResult.Parser, new string[10] { "SinglePos", "SingleStrUtf8", "SingleRate", "OneResult", "Left", "Top", "Right", "Bottom", "Unknown0", "UnknownPos" }, (string[])null, (Type[])null, (Extension[])null, new GeneratedClrTypeInfo[1]
					{
						new GeneratedClrTypeInfo(typeof(OcrResponse.Types.OcrResult.Types.SingleResult.Types.OneResult), (MessageParser)(object)OcrResponse.Types.OcrResult.Types.SingleResult.Types.OneResult.Parser, new string[2] { "OnePos", "OneStrUtf8" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
					})
				})
			}),
			new GeneratedClrTypeInfo(typeof(OcrRequest), (MessageParser)(object)OcrRequest.Parser, new string[3] { "Unknow", "TaskId", "PicPath" }, (string[])null, (Type[])null, (Extension[])null, new GeneratedClrTypeInfo[1]
			{
				new GeneratedClrTypeInfo(typeof(OcrRequest.Types.PicPaths), (MessageParser)(object)OcrRequest.Types.PicPaths.Parser, new string[1] { "PicPath" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
			})
		}));
	}
}
