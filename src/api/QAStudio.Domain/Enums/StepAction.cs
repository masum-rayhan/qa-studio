namespace QAStudio.Domain.Enums;

public enum StepAction
{
    Goto = 0,
    Click = 1,
    Fill = 2,
    Check = 3,
    Uncheck = 4,
    SelectOption = 5,
    Press = 6,
    Hover = 7,
    Scroll = 8,
    DragAndDrop = 9,
    WaitForSelector = 10,
    WaitForURL = 11,
    WaitForNavigation = 12,
    WaitForTimeout = 13,
    AssertVisible = 14,
    AssertHidden = 15,
    AssertText = 16,
    AssertContainsText = 17,
    AssertURL = 18,
    AssertTitle = 19,
    Screenshot = 20
}
