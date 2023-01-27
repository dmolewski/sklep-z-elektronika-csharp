using Allure.Commons;
using NUnit.Allure.Core;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace SeleniumTests
{
    [AllureNUnit]
    class CartTests
    {
        IWebDriver driver;

        IJavaScriptExecutor js;

        IWebElement AddToCartButton => driver.FindElement(By.CssSelector("[name='add-to-cart']"), 2);
        IWebElement GoToCartButton => driver.FindElement(By.CssSelector(".woocommerce-message .wc-forward"), 2);
        IWebElement CartTable => driver.FindElement(By.CssSelector("table.shop_table.cart"), 2);
        IList<IWebElement> CartItems => driver.FindElements(By.CssSelector("tr.cart_item"), 2);
        IWebElement QuantityField => driver.FindElement(By.CssSelector("input.qty"), 2);
        IWebElement UpdateCartButton => driver.FindElement(By.CssSelector("[name='update_cart']"), 2);
        By Loaders => By.CssSelector(".blockUI");

        IList<string> productsIDs = new List<string>() { "12", "12" };

        string baseURL = "http://zelektronika.store";
        IList<string> productsURLs = new List<string>() {
            "/product/hoodie-with-logo",
            "/product/hoodie-with-logo"
        };

        IWebElement DismissNoticeLink => driver.FindElement(By.CssSelector(".woocommerce-store-notice__dismiss-link"));

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);

            js = (IJavaScriptExecutor)driver;
            driver.Navigate().GoToUrl(baseURL);
            DismissNoticeLink.Click();
        }

        [TearDown]
        public void QuitDriver()
        {
            if (TestContext.CurrentContext.Result.Outcome != ResultState.Success)
            {
                string screenshotPath = TakeScreenshot();
                Console.WriteLine("Screenshot: " + screenshotPath);
                AllureLifecycle.Instance.AddAttachment(screenshotPath);
            }
            driver.Quit();
        }

        [Test]
        public void ProductAddedToCartTest()
        {
            driver.Navigate().GoToUrl(baseURL + productsURLs[0]);
            AddToCartButton.Click();
            GoToCartButton.Click();
            _ = CartTable;
            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, CartItems.Count, "Number of product in cart is not 1");
                Assert.AreEqual(productsIDs[0], CartItems[0].FindElement(By.CssSelector("a")).GetAttribute("data-product_id"),
                    "Product's in cart id is not " + productsIDs[0]);
            });
        }

        [Test]
        public void TwoItemsOfProductAddedToCartTest()
        {
            driver.Navigate().GoToUrl(baseURL + productsURLs[0]);
            QuantityField.Clear();
            QuantityField.SendKeys("2");
            AddToCartButton.Click();
            GoToCartButton.Click();
            _ = CartTable;
            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, CartItems.Count, "Number of product in cart is not 1");
                Assert.AreEqual(productsIDs[0], CartItems[0].FindElement(By.CssSelector("a")).GetAttribute("data-product_id"),
                    "Product's in cart id is not " + productsIDs[0]);
                Assert.AreEqual("2", QuantityField.GetAttribute("value"), "Number of items of the product is not 2");
            });
        }

        [Test]
        public void TwoProductsAddedToCartTest()
        {
            driver.Navigate().GoToUrl(baseURL + productsURLs[0]);
            AddToCartButton.Click();
            _ = GoToCartButton;
            driver.Navigate().GoToUrl(baseURL + productsURLs[1]);
            AddToCartButton.Click();
            GoToCartButton.Click();
            _ = CartTable;
            Assert.Multiple(() =>
            {
                Assert.AreEqual(2, CartItems.Count, "Number of product in cart is not 1");
                Assert.AreEqual(productsIDs[0], CartItems[0].FindElement(By.CssSelector("a")).GetAttribute("data-product_id"),
                    "Product's in cart id is not " + productsIDs[0]);
                Assert.AreEqual(productsIDs[1], CartItems[1].FindElement(By.CssSelector("a")).GetAttribute("data-product_id"),
                    "Product's in cart id is not " + productsIDs[1]);
            });
        }

        [Test]
        public void CartEmptyAtStartTest()
        {
            driver.Navigate().GoToUrl(baseURL + "/koszyk/");
            Assert.DoesNotThrow(() => driver.FindElement(By.CssSelector(".cart-empty.woocommerce-info")), "There is no \"Empty Cart\" message");

        }

        [Test]
        public void CantAddZeroItemsTest()
        {
            driver.Navigate().GoToUrl(baseURL + productsURLs[0]);
            QuantityField.Clear();
            QuantityField.SendKeys("0");
            AddToCartButton.Click();

            bool isNotPositiveNumber = (bool)js.ExecuteScript("return arguments[0].validity.rangeUnderflow", QuantityField);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(isNotPositiveNumber, "Test was probably able to add 0 items to cart. Range Underflow validation didn't return \"true\".");
                Assert.Throws<WebDriverTimeoutException>(() => _ = GoToCartButton,
                    "\"Go to cart\" link was found, but it shouldn't. Nothing should be added to cart when you try add 0 items.");
            });
        }

        [Test]
        public void CanRemoveProductFromCartTest()
        {
            driver.Navigate().GoToUrl(baseURL + productsURLs[0]);
            AddToCartButton.Click();
            GoToCartButton.Click();
            _ = CartTable;
            CartItems[0].FindElement(By.CssSelector("a[data-product_id='" + productsIDs[0] + "']")).Click();
            waitForElementsDisappear(Loaders);

            Assert.DoesNotThrow(() => driver.FindElement(By.CssSelector(".cart-empty.woocommerce-info")), "There is no \"Empty Cart\" message. Product was not removed from cart.");
        }

        [Test]
        public void CanIncreaseNumberOfItemsTest()
        {
            driver.Navigate().GoToUrl(baseURL + productsURLs[0]);
            AddToCartButton.Click();
            GoToCartButton.Click();
            _ = CartTable;
            QuantityField.Clear();
            QuantityField.SendKeys("5");
            UpdateCartButton.Click();
            waitForElementsDisappear(Loaders);
            Assert.AreEqual("5", QuantityField.GetAttribute("value"), "Number of items didn't change");
        }

        [Test]
        public void ChangingNumberOfItemsToZeroRemovesProductTest()
        {
            driver.Navigate().GoToUrl(baseURL + productsURLs[0]);
            AddToCartButton.Click();
            GoToCartButton.Click();
            _ = CartTable;
            QuantityField.Clear();
            QuantityField.SendKeys("0");
            UpdateCartButton.Click();
            waitForElementsDisappear(Loaders);
            Assert.DoesNotThrow(() => driver.FindElement(By.CssSelector(".cart-empty.woocommerce-info")), "There is no \"Empty Cart\" message. Product was not removed from cart.");
        }

        [Test]
        public void CantChangeToMoreThanStockTest()
        {
            driver.Navigate().GoToUrl(baseURL + productsURLs[0]);
            string stock = driver.FindElement(By.CssSelector("p.in-stock")).Text.Replace(" w magazynie", "");
            int.TryParse(stock, out int stockNumber);
            AddToCartButton.Click();
            GoToCartButton.Click();
            _ = CartTable;
            QuantityField.Clear();
            QuantityField.SendKeys((stockNumber + 1).ToString());
            UpdateCartButton.Click();
            waitForElementsDisappear(Loaders);
            bool isMoreThanStock = (bool)js.ExecuteScript("return arguments[0].validity.rangeOverflow", QuantityField);
            Assert.IsTrue(isMoreThanStock, "Test was probably able to add more items than available in stock. Range Overflow validation didn't return \"true\".");
        }

        private string TakeScreenshot()
        {
            Screenshot image = ((ITakesScreenshot)driver).GetScreenshot();
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd HHmmss");
            string directoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\screenshots\";
            Directory.CreateDirectory(directoryPath);
            string fullPath = directoryPath + dateTime + ".png";
            image.SaveAsFile(fullPath);
            return fullPath;
        }

        private void waitForElementsDisappear(By by)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            try
            {
                wait.Until(d => driver.FindElements(by).Count == 0);
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("Elements located by " + by + " didn't disappear in 5 seconds.");
                throw;
            }
        }
    }

}