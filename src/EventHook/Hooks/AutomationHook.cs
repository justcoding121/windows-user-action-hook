using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Automation;

namespace AgentSafeX.Client.Utility.Hooks
{
    public class AutomationHook
    {

        public AutomationElement GetUrlFromExplorerWithIdentifier(string WindowsName, IntPtr ptr)
        {
            AutomationElement URL = null;
            
            try
            {
                var aeBrowser = AutomationElement.FromHandle(ptr);
                switch (WindowsName)
                {
                    case "8":
                        // URL = aeBrowser == null ? null : GetURLfromBrowser(aeBrowser, "Go to a Website", "", ControlType.Edit, "edit");
                        break;
                    case "7":
                        URL = aeBrowser == null ? null : GetURLfromExplorer(aeBrowser, ControlType.ToolBar, "ToolbarWindow32", ControlType.Edit, "edit", "Address");
                        break;
                    case "Vista":
                        //  URL = aeBrowser == null ? null : GetURLfromBrowser(aeBrowser, "Address", "Edit", ControlType.Edit, "edit");
                        break;
                    case "XP":
                        // URL = aeBrowser == null ? null : GetURLfromBrowser(aeBrowser, "Address", "Chrome_OmniboxView", ControlType.Edit, "edit");
                        break;

                    default:
                        break;
                }
                return URL;

            }
            catch
            {
                return null;
            }
        }

        public AutomationElement GetURLfromExplorer(AutomationElement rootElement, ControlType ControlTypeName1, string ClassName1, ControlType ControlTypeName2, string ClassName2, string Name2)
        {
            try
            {
                System.Windows.Automation.Condition condition1 = new AndCondition(new PropertyCondition(AutomationElement.ControlTypeProperty, ControlTypeName1), new PropertyCondition(AutomationElement.ClassNameProperty, ClassName1));
                System.Windows.Automation.Condition condition2 = new AndCondition(new PropertyCondition(AutomationElement.ControlTypeProperty, ControlTypeName2), new PropertyCondition(AutomationElement.ClassNameProperty, ClassName2), new PropertyCondition(AutomationElement.NameProperty, Name2));


                var walker = new TreeWalker(new AndCondition(new OrCondition(condition1, condition2), new NotCondition(new PropertyCondition(AutomationElement.NameProperty, ""))));

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
        public String GetUrlFromBrowsersWithIdentifier(string BrowserName, IntPtr ptr)
        {
            String URL = null;

            if (BrowserName == null) return null;
            try
            {
                var aeBrowser = AutomationElement.FromHandle(ptr);
                switch (BrowserName)
                {
                    case "firefox.exe":
                        URL = aeBrowser == null ? null : GetURLfromBrowser(ref aeBrowser, string.Empty, string.Empty, ControlType.Edit, "edit");
                        break;
                    case "opera.exe":
                        URL = aeBrowser == null ? null : GetURLfromBrowser(ref aeBrowser, "Address field", string.Empty, ControlType.Edit, "edit");
                        break;
                    case "iexplore.exe":
                        URL = aeBrowser == null ? null : GetURLfromBrowser(ref aeBrowser, string.Empty, string.Empty, ControlType.Edit, "edit");
                        break;
                    case "chrome.exe":
                        URL = aeBrowser == null ? null : GetURLfromBrowser(ref aeBrowser, string.Empty, string.Empty, ControlType.Edit, "edit");
                        break;
                    default:
                        break;
                }
                return URL;

            }
            catch
            {
                return null;
            }
        }




        public string GetURLfromBrowser(ref AutomationElement rootElement, string Name, string ClassName, ControlType ControlTypeName, string LocalizedControlTypeName)
        {
            try
            {
                var conditions = new List<System.Windows.Automation.Condition>();

                if (Name != string.Empty) conditions.Add(new PropertyCondition(AutomationElement.NameProperty, Name));
                if (ClassName != string.Empty) conditions.Add(new PropertyCondition(AutomationElement.ClassNameProperty, ClassName));
                if (ControlTypeName != null) conditions.Add(new PropertyCondition(AutomationElement.ControlTypeProperty, ControlTypeName));
                if (LocalizedControlTypeName != string.Empty) conditions.Add(new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, LocalizedControlTypeName));

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
