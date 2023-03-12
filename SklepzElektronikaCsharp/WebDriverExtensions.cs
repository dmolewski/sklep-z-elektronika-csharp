using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Collections.ObjectModel;

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