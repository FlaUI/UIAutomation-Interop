using System.Text.RegularExpressions;

private void FixIL(string file) {
    // Read the file
    var content = System.IO.File.ReadAllText(file);

    // Setup
    var lastReplaceCount = 0;

    // Fixes for RegisterPatternAbstract
    content = ReplaceWithCount(content,
@"
  .method public hidebysig newslot abstract virtual 
          instance void  RegisterPattern([in] valuetype Interop.UIAutomationCore.UIAutomationPatternInfo& pattern,
                                         [out] int32& pPatternId,
                                         [out] int32& pPatternAvailablePropertyId,
                                         [in] uint32 propertyIdCount,
                                         [out] int32& pPropertyIds,
                                         [in] uint32 eventIdCount,
                                         [out] int32& pEventIds) runtime managed internalcall
",
@"
  .method public hidebysig newslot abstract virtual 
          instance void  RegisterPattern([in] valuetype Interop.UIAutomationCore.UIAutomationPatternInfo& pattern,
                                         [out] int32& pPatternId,
                                         [out] int32& pPatternAvailablePropertyId,
                                         [in] uint32 propertyIdCount,
                                         [out] int32[] marshal([+3]) pPropertyIds,
                                         [in] uint32 eventIdCount,
                                         [out] int32[] marshal([+5]) pEventIds) runtime managed internalcall
",
    out lastReplaceCount);
    CheckLastReplacement(lastReplaceCount, "RegisterPatternAbstract");

    // Fixes for RegisterPattern
    content = ReplaceWithCount(content,
@"
  .method public hidebysig newslot virtual 
          instance void  RegisterPattern([in] valuetype Interop.UIAutomationCore.UIAutomationPatternInfo& pattern,
                                         [out] int32& pPatternId,
                                         [out] int32& pPatternAvailablePropertyId,
                                         [in] uint32 propertyIdCount,
                                         [out] int32& pPropertyIds,
                                         [in] uint32 eventIdCount,
                                         [out] int32& pEventIds) runtime managed internalcall
",
@"
  .method public hidebysig newslot virtual 
          instance void  RegisterPattern([in] valuetype Interop.UIAutomationCore.UIAutomationPatternInfo& pattern,
                                         [out] int32& pPatternId,
                                         [out] int32& pPatternAvailablePropertyId,
                                         [in] uint32 propertyIdCount,
                                         [out] int32[] marshal([+3]) pPropertyIds,
                                         [in] uint32 eventIdCount,
                                         [out] int32[] marshal([+5]) pEventIds) runtime managed internalcall
",
    out lastReplaceCount);
    CheckLastReplacement(lastReplaceCount, "RegisterPattern");

    // Fixes for CallMethod
    content = ReplaceWithCount(content,
@"
  .method public hidebysig newslot abstract virtual 
          instance void  CallMethod([in] uint32 index,
                                    [in] valuetype Interop.UIAutomationCore.UIAutomationParameter& pParams,
                                    [in] uint32 cParams) runtime managed internalcall
",
@"
  .method public hidebysig newslot abstract virtual 
          instance int32 CallMethod([in] uint32 index,
                                    [in] valuetype Interop.UIAutomationCore.UIAutomationParameter[] marshal([+2]) pParams,
                                    [in] uint32 cParams) runtime managed internalcall preservesig
",
    out lastReplaceCount);
    CheckLastReplacement(lastReplaceCount, "CallMethod");

    // Fixes for GetProperty
    content = ReplaceWithCount(content,
@"
  .method public hidebysig newslot abstract virtual 
          instance void  GetProperty([in] uint32 index,
                                     [in] int32 cached,
                                     [in] valuetype Interop.UIAutomationCore.UIAutomationType 'type',
                                     [out] native int pPtr) runtime managed internalcall
",
@"
  .method public hidebysig newslot abstract virtual 
          instance int32 GetProperty([in] uint32 index,
                                     [in] int32 cached,
                                     [in] valuetype Interop.UIAutomationCore.UIAutomationType 'type',
                                     [out] native int pPtr) runtime managed internalcall preservesig
",
    out lastReplaceCount);
    CheckLastReplacement(lastReplaceCount, "GetProperty");

    // Fixes for Dispatch
    content = ReplaceWithCount(content,
@"
  .method public hidebysig newslot abstract virtual 
          instance void  Dispatch([in] object  marshal( iunknown ) pTarget,
                                  [in] uint32 index,
                                  [in] valuetype Interop.UIAutomationCore.UIAutomationParameter& pParams,
                                  [in] uint32 cParams) runtime managed internalcall
",
@"
  .method public hidebysig newslot abstract virtual 
          instance void  Dispatch([in] object  marshal( iunknown ) pTarget,
                                  [in] uint32 index,
                                  [in] valuetype Interop.UIAutomationCore.UIAutomationParameter[] marshal([+3]) pParams,
                                  [in] uint32 cParams) runtime managed internalcall
",
    out lastReplaceCount);
    CheckLastReplacement(lastReplaceCount, "Dispatch");

    // Fixes for UIAutomationPropertyInfo
    content = ReplaceWithCount(content,
@"
.class public sequential ansi sealed beforefieldinit Interop.UIAutomationCore.UIAutomationPropertyInfo
       extends [mscorlib]System.ValueType
{
  .pack 4
",
@"
.class public sequential ansi sealed beforefieldinit Interop.UIAutomationCore.UIAutomationPropertyInfo
       extends [mscorlib]System.ValueType
{
  .pack 0
",
    out lastReplaceCount);
    CheckLastReplacement(lastReplaceCount, "UIAutomationPropertyInfo");

    // Fixes for UIAutomationEventInfo
    content = ReplaceWithCount(content,
@"
.class public sequential ansi sealed beforefieldinit Interop.UIAutomationCore.UIAutomationEventInfo
       extends [mscorlib]System.ValueType
{
  .pack 4
",
@"
.class public sequential ansi sealed beforefieldinit Interop.UIAutomationCore.UIAutomationEventInfo
       extends [mscorlib]System.ValueType
{
  .pack 0
",
    out lastReplaceCount);
    CheckLastReplacement(lastReplaceCount, "UIAutomationEventInfo");

    // Fixes for UIAutomationPatternInfo
    content = ReplaceWithCount(content,
@"
.class public sequential ansi sealed beforefieldinit Interop.UIAutomationCore.UIAutomationPatternInfo
       extends [mscorlib]System.ValueType
{
  .pack 4
",
@"
.class public sequential ansi sealed beforefieldinit Interop.UIAutomationCore.UIAutomationPatternInfo
       extends [mscorlib]System.ValueType
{
  .pack 0
",
    out lastReplaceCount);
    CheckLastReplacement(lastReplaceCount, "UIAutomationPatternInfo");

    // Fixes for UIAutomationParameter
    content = ReplaceWithCount(content,
@"
.class public sequential ansi sealed beforefieldinit Interop.UIAutomationCore.UIAutomationParameter
       extends [mscorlib]System.ValueType
{
  .pack 4
",
@"
.class public sequential ansi sealed beforefieldinit Interop.UIAutomationCore.UIAutomationParameter
       extends [mscorlib]System.ValueType
{
  .pack 0
",
    out lastReplaceCount);
    CheckLastReplacement(lastReplaceCount, "UIAutomationParameter");

    // Fixes for UIAutomationMethodInfo
    content = ReplaceWithCount(content,
@"
.class public sequential ansi sealed beforefieldinit Interop.UIAutomationCore.UIAutomationMethodInfo
       extends [mscorlib]System.ValueType
{
  .pack 4
",
@"
.class public sequential ansi sealed beforefieldinit Interop.UIAutomationCore.UIAutomationMethodInfo
       extends [mscorlib]System.ValueType
{
  .pack 0
",
    out lastReplaceCount);
    CheckLastReplacement(lastReplaceCount, "UIAutomationMethodInfo");

    // Write the file back
    System.IO.File.WriteAllText(file, content);
}

private string ReplaceWithCount(string input, string pattern, string replacement, out int numberOfReplacements)
{
    int matchCount = 0;
    string newText = Regex.Replace(input, Regex.Escape(pattern),
        (match) =>
        {
            matchCount++;
            return match.Result(replacement);
        });
    numberOfReplacements = matchCount;
    return newText;
}

private void CheckLastReplacement(int value, string message) {
    if (value != 1) {
        throw new Exception($"Replacement in {message} failed, replaced {value} instead of 1.");
    }
}
