using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Text;
using System.Threading;

namespace SeleniumTests
{
    //https://www.plukasiewicz.net/Artykuly/Metody_rozszerzajace
    public static class WebDriverExtensions
    {
        public static IWebElement FindElement(this ISearchContext context, By by, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                DefaultWait<ISearchContext> wait = new DefaultWait<ISearchContext>(context);
                wait.IgnoreExceptionTypes(typeof(NoSuchElementException));
                wait.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
                return wait.Until(c => c.FindElement(by));
            }
            return context.FindElement(by);

        }

        public static ReadOnlyCollection<IWebElement> FindElements(this ISearchContext context, By by, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                DefaultWait<ISearchContext> wait = new DefaultWait<ISearchContext>(context);
                wait.IgnoreExceptionTypes(typeof(NoSuchElementException));
                wait.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
                return wait.Until(c => (c.FindElements(by).Count > 0) ? c.FindElements(by) : null);
            }
            return context.FindElements(by);
        }
    }
}