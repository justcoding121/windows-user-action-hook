using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Automation;

namespace EventHook.Hooks
{
    public class AutomationHook
    {
        public AutomationElement GetUrlFromExplorerWithIdentifier(string windowsName, IntPtr ptr)
        {
            AutomationElement url = null;

            try
            {
                var aeBrowser = AutomationElement.FromHandle(ptr);
                switch (windowsName)
                {
                    case "8":
                        // URL = aeBrowser == null ? null : GetURLfromBrowser(aeBrowser, "Go to a Website", "", ControlType.Edit, "edit");
                        break;
                    case "7":
                        url = aeBrowser == null
                            ? null
                            : GetUrLfromExplorer(aeBrowser, ControlType.ToolBar, "ToolbarWindow32", ControlType.Edit,
                                "edit", "Address");
                        break;
                    case "Vista":
                        //  URL = aeBrowser == null ? null : GetURLfromBrowser(aeBrowser, "Address", "Edit", ControlType.Edit, "edit");
                        break;
                    case "XP":
                        // URL = aeBrowser == null ? null : GetURLfromBrowser(aeBrowser, "Address", "Chrome_OmniboxView", ControlType.Edit, "edit");
                        break;
                }
                return url;
            }
            catch
            {
                return null;
            }
        }

        public AutomationElement GetUrLfromExplorer(AutomationElement rootElement, ControlType controlTypeName1,
            string className1, ControlType controlTypeName2, string className2, string name2)
        {
            try
            {
                Condition condition1 =
                    new AndCondition(new PropertyCondition(AutomationElement.ControlTypeProperty, controlTypeName1),
                        new PropertyCondition(AutomationElement.ClassNameProperty, className1));
                Condition condition2 =
                    new AndCondition(new PropertyCondition(AutomationElement.ControlTypeProperty, controlTypeName2),
                        new PropertyCondition(AutomationElement.ClassNameProperty, className2),
                        new PropertyCondition(AutomationElement.NameProperty, name2));


                var walker =
                    new TreeWalker(new AndCondition(new OrCondition(condition1, condition2),
                        new NotCondition(new PropertyCondition(AutomationElement.NameProperty, ""))));

                var elementNode = walker.GetFirstChild(rootElement);

                if (elementNode != null)
                {
                    return elementNode;
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        public string GetUrlFromBrowsersWithIdentifier(string browserName, IntPtr ptr)
        {
            string url = null;

            if (browserName == null) return null;
            try
            {
                var aeBrowser = AutomationElement.FromHandle(ptr);
                switch (browserName)
                {
                    case "firefox.exe":
                        url = aeBrowser == null
                            ? null
                            : GetUrLfromBrowser(ref aeBrowser, string.Empty, string.Empty, ControlType.Edit, "edit");
                        break;
                    case "opera.exe":
                        url = aeBrowser == null
                            ? null
                            : GetUrLfromBrowser(ref aeBrowser, "Address field", string.Empty, ControlType.Edit, "edit");
                        break;
                    case "iexplore.exe":
                        url = aeBrowser == null
                            ? null
                            : GetUrLfromBrowser(ref aeBrowser, string.Empty, string.Empty, ControlType.Edit, "edit");
                        break;
                    case "chrome.exe":
                        url = aeBrowser == null
                            ? null
                            : GetUrLfromBrowser(ref aeBrowser, string.Empty, string.Empty, ControlType.Edit, "edit");
                        break;
                }
                return url;
            }
            catch
            {
                return null;
            }
        }


        public string GetUrLfromBrowser(ref AutomationElement rootElement, string name, string className,
            ControlType controlTypeName, string localizedControlTypeName)
        {
            try
            {
                var conditions = new List<Condition>();

                if (name != string.Empty) conditions.Add(new PropertyCondition(AutomationElement.NameProperty, name));
                if (className != string.Empty)
                    conditions.Add(new PropertyCondition(AutomationElement.ClassNameProperty, className));
                if (controlTypeName != null)
                    conditions.Add(new PropertyCondition(AutomationElement.ControlTypeProperty, controlTypeName));
                if (localizedControlTypeName != string.Empty)
                    conditions.Add(new PropertyCondition(AutomationElement.LocalizedControlTypeProperty,
                        localizedControlTypeName));

                var walker = new TreeWalker(new AndCondition(conditions.ToArray()));
                var elementNode = walker.GetFirstChild(rootElement);
                if (elementNode != null)
                {
                    var p = elementNode.GetSupportedPatterns();
                    if (p.Any(autop => autop.ProgrammaticName.Equals("ValuePatternIdentifiers.Pattern")))
                    {
                        var valuePattern = elementNode.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;
                        if (valuePattern != null)
                        {
                            return (valuePattern.Current.Value);
                        }
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }
    }
}