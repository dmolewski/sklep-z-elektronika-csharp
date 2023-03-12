using Allure.Commons;
using NLog;
using NUnit.Allure.Core;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SeleniumTests
{
    [AllureNUnit]
    class CartTests

    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        IWebDriver driver;

        IJavaScriptExecutor js;

        IWebElement DismissNoticeLink => driver.FindElement(By.CssSelector(".woocommerce-store-notice__dismiss-link"));
        IWebElement AddToCartButton => driver.FindElement(By.CssSelector("[name='add-to-cart']"), 2);
        IWebElement GoToCartButton => driver.FindElement(By.CssSelector(".woocommerce-message .wc-forward"), 2);
        IWebElement CartTable => driver.FindElement(By.CssSelector("table.shop_table.cart"), 2);
        IList<IWebElement> CartItems => driver.FindElements(By.CssSelector("tr.cart_item"), 2);
        IWebElement QuantityField => driver.FindElement(By.CssSelector("input.qty"), 2);
        IWebElement UpdateCartButton => driver.FindElement(By.CssSelector("[name='update_cart']"), 2);

        By ProductPageAddToCartButton = By.CssSelector("button[name='add-to-cart']");
        By ProductPageViewCartButton = By.CssSelector(".woocommerce-message>.button");
        By ShopTable = By.CssSelector(".shop_table");
        By RemoveProductButton = By.CssSelector("a[data-product_id='" + productId + "']");

        By checkoutButton = By.CssSelector(".checkout-button");
        By orderButton = By.CssSelector("#place_order");


        By Loaders => By.CssSelector(".blockUI");

        By summaryDate = By.CssSelector(".date>strong");
        By summaryPrice = By.CssSelector(".total .amount");
        By summaryPaymentMethod = By.CssSelector(".method>strong");
        By summaryProductRows = By.CssSelector("tbody>tr");
        By summaryProductQuantity = By.CssSelector(".product-quantity");
        By summaryProductName = By.CssSelector(".product-name>a");

        By firstNameField = By.CssSelector("#billing_first_name");
        By lastNameField = By.CssSelector("#billing_last_name");
        By countryCodeArrow = By.CssSelector(".select2-selection__arrow");
        By addressField = By.CssSelector("#billing_address_1");
        By postalCodeField = By.CssSelector("#billing_postcode");
        By cityField = By.CssSelector("#billing_city");
        By phoneField = By.CssSelector("#billing_phone");
        By emailField = By.CssSelector("#billing_email");
        By paymentMethod = By.CssSelector("label[for='payment_method_stripe']");
        By shippingMethod = By.CssSelector("label[for='shipping_method_0_flat_rate2']");
        By loadingIcon = By.CssSelector(".blockOverlay");
        By cardNumberFrame = By.CssSelector("#stripe-card-element iframe");
        By cardNumberField = By.CssSelector("[name='cardnumber']");
        By expirationDateFrame = By.CssSelector("#stripe-exp-element iframe");
        By expirationDateField = By.CssSelector("[name='exp-date']");
        By cvcFrame = By.CssSelector("#stripe-cvc-element iframe");
        By cvcField = By.CssSelector("[name='cvc']");


        private static readonly string storeURL = "http://zelektronika.store/";
        private static readonly string username = "dmolewski+sklep";
        private static readonly string password = "testowekontoztestowymhaslem";
        private static readonly string userFullName = "dmolewskisklep";

        private static readonly string productId = "192";


        private static readonly Random random = new Random();
        public static int randomNumber = random.Next(1, 101);

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);

            js = (IJavaScriptExecutor)driver;
            driver.Navigate().GoToUrl(storeURL);
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
        private static string GetCurrentDate()
        {
            return DateTime.Now.ToString("d MMMM yyyy");
        }

        private void SwitchToFrame(By frameLocator)
        {
            WebDriverWait Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            Wait.Until(ExpectedConditions.FrameToBeAvailableAndSwitchToIt(frameLocator));
            Wait.Until(d => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
        }

        private void SlowType(IWebElement element, string text)
        {
            foreach (char character in text)
            {
                element.SendKeys(character.ToString());
                Thread.Sleep(50);
            }
        }

        private void Register(string email, string password)
        {
            driver.Navigate().GoToUrl(storeURL);
            driver.FindElement(By.CssSelector("li[id='menu-item-19']")).Click();
            driver.FindElement(By.CssSelector("input[id='reg_email']")).SendKeys(email);
            driver.FindElement(By.CssSelector("input[id='reg_password']")).SendKeys(password);
            driver.FindElement(By.CssSelector("button[name='register']")).Click();
        }
        private string GetAccountMessage()
        {
            return driver.FindElement(By.CssSelector("div[class='woocommerce-MyAccount-content']>p")).Text;
        }

        private void DeleteAccount()
        {
            driver.FindElement(By.CssSelector("a[href='http://zelektronika.store/my-account/wpf-delete-account/']")).Click();
            driver.FindElement(By.CssSelector("div[class='wpfda-submit'] button[type='submit']")).Click();
        }

        private void GoToMyAccountSubpage(string selector, string expectedText)
        {
            ClickAndWait(By.CssSelector("li[id='menu-item-19']"));
            ClickAndWait(By.CssSelector(selector));
            string subpageContent = driver.FindElement(By.CssSelector("div[class='woocommerce-MyAccount-content']>p")).Text;
            log.Info($"Znaleziony tekst: \"{subpageContent}\", oczekiwany: \"{expectedText}\"");
            string errorMessage = string.Format("Strona nie zawiera spodziewanego fragmentu tekstu. Spodziewany fragment: \"%s\", znaleziony tekst: \"%s\"", expectedText, subpageContent);
            Assert.IsTrue(subpageContent.Contains(expectedText), errorMessage);
        }

        private void ClickAndWait(By selector)
        {
            driver.FindElement(selector).Click();
        }

        private void SearchForProduct(string productName)
        {
            IWebElement searchBox = driver.FindElement(By.CssSelector(".woocommerce-product-search input"));
            searchBox.SendKeys(productName);

            Actions actions = new Actions(driver);
            actions.MoveToElement(searchBox);
            actions.Click();
            actions.SendKeys(Keys.Enter).Perform();

            //searchBox.SendKeys(productName, Keys.RETURN); //tylko dla Chrome, Firefox wymusza u¿ycie klasy Actions w przypadku u¿ycia klawisza ENTER/RETURN
        }

        private void SortByPriceLowToHigh()
        {
            IWebElement sortByDropdown = driver.FindElement(By.CssSelector(".woocommerce-ordering select"));
            sortByDropdown.Click();

            SelectElement select = new SelectElement(sortByDropdown);
            select.SelectByText("Sortuj po cenie od najni¿szej");
        }

        private List<double> GetProductPrices()
        {
            IList<IWebElement> productPriceElements = driver.FindElements(By.CssSelector(".price"));
            List<double> productPrices = new List<double>();
            foreach (IWebElement productPriceElement in productPriceElements)
            {
                string productPriceText = productPriceElement.Text.Replace(",", ".").Replace("z³", "").Trim();
                double productPrice = double.Parse(productPriceText, CultureInfo.InvariantCulture);
                productPrices.Add(productPrice);
            }
            return productPrices;
        }

        private void AddProductToCart(string productPageUrl)
        {
            driver.Navigate().GoToUrl(productPageUrl);
            AddProductToCart();
            AddToCartCheck();
        }

        private void AddProductToCart()
        {
            IWebElement addToCartButton = driver.FindElement(ProductPageAddToCartButton);
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", addToCartButton);
            addToCartButton.Click();
            driver.FindElement(ProductPageViewCartButton);
        }

        private void AddToCartCheck()
        {
            string productName = driver.FindElement(By.CssSelector(".product_title.entry-title")).Text;

            Assert.IsTrue(WaitForMessage().Contains("„" + productName + "” zosta³ dodany do koszyka."), "Produkt „" + productName + "” nie zosta³ dodany do koszyka");
            log.Info("Dodano nowy produkt do koszyka: „" + productName + "”");
        }

        private string WaitForMessage()
        {
            WebDriverWait Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            Wait.Until(d => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
            var messageElements = Wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector(".woocommerce-message")));
            var messages = messageElements.Select(e => e.Text);
            return string.Join(Environment.NewLine, messages);
            //return Wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector(".woocommerce-message"))).Text;
        }

        private string WaitForErrorMessage()
        {
            WebDriverWait Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            By errorList = By.CssSelector("ul.woocommerce-error");
            Wait.Until(d => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
            return Wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(errorList))[0].Text;
        }

        private double AddProductPrice()
        {
            WebDriverWait Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            string productPriceText = driver.FindElement(By.CssSelector("div.summary.entry-summary bdi:nth-child(1)")).Text;
            double productPrice = double.Parse(Regex.Replace(productPriceText, @"[^0-9.,]+", "").Replace(",", "."), CultureInfo.InvariantCulture);
            Wait.Until(ExpectedConditions.ElementExists(By.CssSelector("div#content p.woocommerce-mini-cart__total.total bdi:nth-child(1)")));
            return productPrice;
        }

        private void ViewCart()
        {
            WebDriverWait Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            Wait.Until(ExpectedConditions.ElementToBeClickable(ProductPageViewCartButton)).Click();
            Wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(ShopTable));
        }

        private double GetCartTotalPrice()
        {
            WebDriverWait Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            string cartTotalPriceText;
            Wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(".blockOverlay")));
            Wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("tr[class='cart-subtotal'] bdi:nth-child(1)")));
            cartTotalPriceText = driver.FindElement(By.CssSelector("tr[class='cart-subtotal'] bdi:nth-child(1)")).Text.Replace(",", ".");
            return double.Parse(cartTotalPriceText.Replace("z³", ""), CultureInfo.InvariantCulture);
        }

        private double GetCartTotalPriceWithCoupon()
        {
            WebDriverWait Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            string cartTotalPriceText;
            Wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(".blockOverlay")));
            Wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("tr[class='order-total'] bdi:nth-child(1)")));
            cartTotalPriceText = driver.FindElement(By.CssSelector("tr[class='order-total'] bdi:nth-child(1)")).Text.Replace(",", ".");
            return double.Parse(cartTotalPriceText.Replace("z³", ""), CultureInfo.InvariantCulture);
        }

        private void AddCoupon()
        {
            WebDriverWait Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            Wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("#coupon_code"))).SendKeys("rabatwsti");
            Wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button[value='Wykorzystaj kupon']"))).Click();

            string couponMessage = WaitForMessage();

            string expectedCouponMessage = "Kupon zosta³ pomyœlnie u¿yty.";
            Assert.AreEqual(expectedCouponMessage, couponMessage, "Kupon nie zosta³ dodany do koszyka");

            log.Info("Dodano kupon rabatowy");
            Wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(".blockOverlay")));

            IWebElement freeShippingAvailablility = driver.FindElement(By.CssSelector("label[for='shipping_method_0_free_shipping1']"));
            Assert.IsNotNull(freeShippingAvailablility, "Darmowa dostawa jest nie dostêpna");
        }

        public void RemoveProductFromCart()
        {
            WebDriverWait Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            driver.FindElement(RemoveProductButton).Click();
            Wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(".blockOverlay")));
        }

        private void FillOutCheckoutForm(string email, string phone)
        {
            WebDriverWait Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            Wait.Until(ExpectedConditions.ElementToBeClickable(firstNameField)).SendKeys("Helena");
            Wait.Until(ExpectedConditions.ElementToBeClickable(lastNameField)).SendKeys("Mazur");
            Wait.Until(ExpectedConditions.ElementToBeClickable(countryCodeArrow)).Click();
            Wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("li[id*='-PL']"))).Click();
            Wait.Until(ExpectedConditions.ElementToBeClickable(addressField)).Click();
            Wait.Until(ExpectedConditions.ElementToBeClickable(addressField)).Clear();
            Wait.Until(ExpectedConditions.ElementToBeClickable(addressField)).SendKeys("Diamentowa 145");
            Wait.Until(ExpectedConditions.ElementToBeClickable(postalCodeField)).Click();
            Wait.Until(ExpectedConditions.ElementToBeClickable(postalCodeField)).Clear();
            Wait.Until(ExpectedConditions.ElementToBeClickable(postalCodeField)).SendKeys("71-232");
            Wait.Until(ExpectedConditions.ElementToBeClickable(cityField)).SendKeys("Szczecin");
            Wait.Until(ExpectedConditions.ElementToBeClickable(phoneField)).SendKeys(phone);
            Wait.Until(ExpectedConditions.ElementToBeClickable(emailField)).SendKeys(email);
        }

        private void FillOutCardData(string cardNumber, string expirationDate, string cvc)
        {
            WebDriverWait Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            Wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(".blockOverlay")));

            //zamiast zwyk³ego findElement dla wiêkszej stabilnoœci testów w FireFox
            IWebElement element = driver.FindElement(shippingMethod);
            IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
            executor.ExecuteScript("arguments[0].click();", element);

            //driver.FindElement(shippingMethod).click();

            Wait.Until(ExpectedConditions.InvisibilityOfElementLocated(loadingIcon));

            driver.FindElement(paymentMethod).Click();
            Wait.Until(ExpectedConditions.InvisibilityOfElementLocated(loadingIcon));

            SwitchToFrame(cardNumberFrame);
            IWebElement cardNumberElement = Wait.Until(ExpectedConditions.ElementToBeClickable(cardNumberField));
            cardNumberElement.Clear();
            SlowType(cardNumberElement, cardNumber);
            driver.SwitchTo().DefaultContent();

            SwitchToFrame(expirationDateFrame);
            IWebElement expirationDateElement = Wait.Until(ExpectedConditions.ElementToBeClickable(expirationDateField));
            expirationDateElement.Clear();
            SlowType(expirationDateElement, expirationDate);
            driver.SwitchTo().DefaultContent();

            SwitchToFrame(cvcFrame);
            IWebElement cvcElement = Wait.Until(ExpectedConditions.ElementToBeClickable(cvcField));
            cvcElement.Clear();
            SlowType(cvcElement, cvc);
            driver.SwitchTo().DefaultContent();
        }

        private void CheckConfirmationBox()
        {
            WebDriverWait Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            driver.SwitchTo().DefaultContent();
            Wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(".blockOverlay")));
            IWebElement confirmationBox = driver.FindElement(By.CssSelector("input#terms"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", confirmationBox);
            confirmationBox.Click();
            Wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(".blockOverlay")));
        }

        private string OrderAndWaitToComplete()
        {
            WebDriverWait Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            Wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(".blockOverlay")));
            driver.FindElement(orderButton).Click();

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
            wait.Until(ExpectedConditions.UrlContains("/checkout/zamowienie-otrzymane/"));
            string orderNumber = driver.FindElement(By.CssSelector(".order>strong")).Text;
            log.Info("Zamówienie z³o¿one poprawnie - nr zamówienia: " + orderNumber);
            return orderNumber;
        }

        private void LogInDuringCheckout(string userName, string password)
        {
            WebDriverWait Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            By expandLoginForm = By.CssSelector(".showlogin");
            By wrappedLoginView = By.CssSelector(".login[style='display: none;']");

            By usernameField = By.CssSelector("#username");
            By passwordField = By.CssSelector("#password");
            By loginButton = By.CssSelector("[name='login']");

            Wait.Until(ExpectedConditions.ElementToBeClickable(expandLoginForm)).Click();
            Wait.Until(ExpectedConditions.InvisibilityOfElementLocated(wrappedLoginView));
            Wait.Until(ExpectedConditions.ElementToBeClickable(usernameField)).SendKeys(userName);
            Wait.Until(ExpectedConditions.ElementToBeClickable(passwordField)).SendKeys(password);
            driver.FindElement(loginButton).Click();
            log.Info("Zalogowano z nazw¹ u¿ytkownika: " + userName);
        }

        private void GoToMyAccountOrders()
        {
            WebDriverWait Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            driver.FindElement(By.CssSelector("li[id='menu-item-19']")).Click();
            Wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".woocommerce-MyAccount-navigation-link--orders"))).Click();
        }

        private void CheckOrderDetailsSection()
        {
            WebDriverWait Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            Wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.CssSelector("section.woocommerce-order-details")));
            IWebElement orderDetailsSection = driver.FindElement(By.CssSelector("section.woocommerce-order-details"));

            IList<IWebElement> orderDetailsLabels = orderDetailsSection.FindElements(By.CssSelector("th"));
            IList<IWebElement> orderDetailsValues = orderDetailsSection.FindElements(By.CssSelector("td"));

            foreach (IWebElement label in orderDetailsLabels)
            {
                string labelText = label.Text.Trim();
                int index = orderDetailsLabels.IndexOf(label);
                string valueText = orderDetailsValues[index].Text.Trim();
                log.Info(labelText + " " + valueText);
                Assert.IsNotNull(valueText, "Wartoœæ pola: \"" + labelText + "\" jest pusta");
            }

            IWebElement billingAddressSection = driver.FindElement(By.CssSelector("div.woocommerce-column--billing-address"));
            IWebElement billingAddressElement = billingAddressSection.FindElement(By.CssSelector("address"));
            Assert.IsNotNull(billingAddressElement, "Adres rozliczeniowy jest pusty");
            log.Info(billingAddressSection.Text);

            IWebElement shippingAddressSection = driver.FindElement(By.CssSelector("div.woocommerce-column--shipping-address"));
            IWebElement shippingAddressElement = shippingAddressSection.FindElement(By.CssSelector("address"));
            log.Info(shippingAddressSection.Text);
            Assert.IsNotNull(shippingAddressElement, "Adres do wysy³ki jest pusty");
        }

        private void VerifySearchResults(string searchTerm)
        {
            WebDriverWait Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            Wait.Until(ExpectedConditions.UrlContains(searchTerm));
            Wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector(".product")));

            IList<IWebElement> searchResultElements = driver.FindElements(By.CssSelector(".product"));
            int numSearchResults = searchResultElements.Count;

            Assert.IsTrue(numSearchResults > 0, "Brak wyników wyszukiwania dla zadanego zapytania");

            log.Info($"Liczba wyników wyszukiwania: {numSearchResults}");

            foreach (IWebElement searchResultElement in searchResultElements)
            {
                string searchResultText = searchResultElement.Text;
                Assert.IsTrue(searchResultText.ToLower().Contains(searchTerm), $"Wynik wyszukiwania nie zawiera frazy: \"{searchTerm}\"");
            }
            log.Info($"Wszystkie wyniki wyszukiwania zawieraj¹ frazê \"{searchTerm}\"");
        }

        [Test]
        public void RegisterWithEmailAndPassword()
        {
            string email = username + randomNumber + "@gmail.com";
            Register(email, password);

            string myAccountContent = GetAccountMessage();
            string expectedName = userFullName + randomNumber;
            string errorMessage = string.Format("Strona nie zawiera spodziewanej nazwy u¿ytkownika: \"{0}\", znaleziono: \"{1}\"", expectedName, myAccountContent);
            Assert.IsTrue(myAccountContent.Contains(expectedName), errorMessage);
            log.Info(string.Format("Oczekiwana nazwa u¿ytkownika: \"{0}\", strona zawiera nazwê u¿ytkownika: \"{1}\"", expectedName, myAccountContent));

            string ordersSelector = ".woocommerce-MyAccount-navigation-link--orders";
            string editAddressSelector = ".woocommerce-MyAccount-navigation-link--edit-address";
            string paymentMethodsSelector = ".woocommerce-MyAccount-navigation-link--payment-methods";
            Assert.IsTrue(driver.FindElement(By.CssSelector(ordersSelector)).Text.Contains("Zamówienia"), "Strona nie zawiera spodziewanego przycisku: Zamówienia");
            Assert.IsTrue(driver.FindElement(By.CssSelector(editAddressSelector)).Text.Contains("Adresy"), "Strona nie zawiera spodziewanego przycisku: Adresy");
            Assert.IsTrue(driver.FindElement(By.CssSelector(paymentMethodsSelector)).Text.Contains("Metody p³atnoœci"), "Strona nie zawiera spodziewanego przycisku: Metody p³atnoœci");

            GoToMyAccountSubpage(editAddressSelector, "adresy");
            GoToMyAccountSubpage(paymentMethodsSelector, "metod");

            DeleteAccount();
        }

        [Test]
        public void SearchInStoreAndSortResults()
        {
            string productName = "komputer";
            SearchForProduct(productName);
            VerifySearchResults(productName);

            SortByPriceLowToHigh();
            List<double> productPrices = GetProductPrices();

            log.Info($"Na stronie wyników s¹ widoczne ({productPrices.Count}) ceny produktów");
            log.Info($"Ceny produktów: {string.Join(", ", productPrices)}");

            List<double> sortedPrices = new List<double>(productPrices);
            sortedPrices.Sort();
            Assert.AreEqual(sortedPrices, productPrices, "Produkty nie s¹ posortowane od najni¿szej ceny");
            log.Info($"Posortowane ceny produktów: {string.Join(", ", sortedPrices)}");
        }

        [Test]
        public void AddToShoppingCart()
        {
            string[] productPages = { "/drukarka/", "/glosnik/", "/komputer/",
                                    //"/komputer-przenosny/", "/monitor/", "/mysz-komputerowa/", "/sluchawki/", "/tablet/"
                                    };

            double totalPrice = 0.00;

            foreach (string productPage in productPages)
            {
                AddProductToCart("http://zelektronika.store/product" + productPage);
                totalPrice += AddProductPrice();
                log.Info($"Aktualna wartoœæ koszyka: {totalPrice}");
            }

            ViewCart();
            double cartTotalPrice = GetCartTotalPrice();
            Assert.AreEqual(cartTotalPrice, totalPrice, 0.02, $"Cena produktów w koszyku ({GetCartTotalPrice()}) nie jest równa obliczonej w teœcie: {totalPrice}");

            int numberOfItems = driver.FindElements(By.CssSelector(".cart_item")).Count;
            Assert.AreEqual(productPages.Length, numberOfItems, $"Iloœæ produktów w koszyku jest nieprawid³owa. Wymagane: {productPages.Length}, w ramach testu obliczono: {numberOfItems}");

            AddCoupon();

            double cartTotalPriceAfterCoupon = GetCartTotalPriceWithCoupon();
            log.Info($"Cena po rabacie: {cartTotalPriceAfterCoupon}");
            Console.WriteLine($"Cena po rabacie: {cartTotalPriceAfterCoupon}");

            double expectedDiscountedPrice = cartTotalPrice * 0.8;
            Assert.AreEqual(cartTotalPriceAfterCoupon, expectedDiscountedPrice, 0.02, $"Cena po uwzglêdnieniu rabatu nie jest równa oczekiwanej: {cartTotalPriceAfterCoupon} =/= {expectedDiscountedPrice}");

            RemoveProductFromCart();
            numberOfItems = driver.FindElements(By.CssSelector(".cart_item")).Count;
            Assert.AreEqual(productPages.Length - 1, numberOfItems, $"Iloœæ produktów w koszyku jest nieprawid³owa. Wymagane: {productPages.Length}, w ramach testu obliczono: {numberOfItems}");
        }

        [Test]
        public void CheckoutTest()
        {
            AddProductToCart("http://zelektronika.store/product/komputer");
            ViewCart();
            driver.FindElement(checkoutButton).Click();
            string email = $"{username}{randomNumber}@gmail.com";
            FillOutCheckoutForm(email, "123456789");
            FillOutCardData("4242424242424242", "0226", "456");

            CheckConfirmationBox();

            int orderNumber = int.Parse(OrderAndWaitToComplete());

            int numberOfOrderReceivedMessages = driver.FindElements(By.CssSelector(".woocommerce-thankyou-order-received")).Count;
            int expectedNumberOfMessages = 1;
            Assert.AreEqual(expectedNumberOfMessages, numberOfOrderReceivedMessages, "Nieprawid³owy komunikat o otrzymaniu zamówienia, czy p³atnoœæ zosta³a poprawnie przetworzona?");

            string dateFromSummary = driver.FindElement(summaryDate).Text;
            string currentDate = GetCurrentDate();
            string actualPrice = driver.FindElement(summaryPrice).Text;
            string expectedPrice = "2008,99 z³";
            string actualPaymentMethod = driver.FindElement(summaryPaymentMethod).Text;
            string expectedPaymentMethod = "Karta p³atnicza (Stripe)";
            int actualNumberOfProducts = driver.FindElements(summaryProductRows).Count;
            int expectedNumberOfProducts = 1;
            string actualProductQuantity = driver.FindElement(summaryProductQuantity).Text;
            string expectedProductQuantity = "× 1";
            string actualProductName = driver.FindElement(summaryProductName).Text;
            string expectedProductName = "Komputer";

            Assert.Multiple(() =>
            {
                Assert.IsTrue(orderNumber > 0, "Numer zamówienia nie jest wiêkszy ni¿ 0");
                Assert.AreEqual(currentDate, dateFromSummary, $"Data w podsumowaniu nieprawid³owa. Oczekiwana: {currentDate}, w podsumowaniu: {dateFromSummary}");
                Assert.AreEqual(expectedPrice, actualPrice, $"Cena w podsumowaniu nieprawid³owa. Oczekiwana: {expectedPrice}, w podsumowaniu: {actualPrice}");
                Assert.AreEqual(expectedPaymentMethod, actualPaymentMethod, $"Metoda p³atnoœci w podsumowaniu nieprawid³owa. Oczekiwana: {expectedPaymentMethod} w podsumowaniu: {actualPaymentMethod}");
                Assert.AreEqual(expectedNumberOfProducts, actualNumberOfProducts, $"Produkty w podsumowaniu nieprawid³owe. Oczekiwane: {expectedNumberOfProducts} w podsumowaniu: {actualNumberOfProducts}");
                Assert.AreEqual(expectedProductQuantity, actualProductQuantity, $"Liczba produktów w podsumowaniu nieprawid³owa. Oczekiwana: {expectedProductQuantity} w podsumowaniu: {actualProductQuantity}");
                Assert.AreEqual(expectedProductName, actualProductName, $"Nazwa produktu w podsumowaniu nieprawid³owa. Oczekiwana: {expectedProductName} w podsumowaniu: {actualProductName}");
                log.Info("Dane w podsumowaniu zamówienia s¹ poprawne");
            });
        }

        [Test]
        public void PaymentTest()
        {
            AddProductToCart("http://zelektronika.store/product/komputer");
            ViewCart();
            driver.FindElement(checkoutButton).Click();
            string email = username + randomNumber + "@gmail.com";
            FillOutCheckoutForm(email, "123456789");

            FillOutCardData("4000000000000002", "0123", "456");
            CheckConfirmationBox();
            driver.FindElement(orderButton).Click();

            string actualErrorMessage = WaitForErrorMessage();
            log.Info("Wyœwietlony b³¹d: " + actualErrorMessage);
            Assert.IsTrue(actualErrorMessage.Contains("Data wa¿noœci karty ju¿ minê³a."), "B³¹d daty wa¿noœci karty nie zosta³ wyœwietlony");

            FillOutCardData("4000000000000002", "0227", "");
            driver.FindElement(orderButton).Click();
            actualErrorMessage = WaitForErrorMessage();
            log.Info("Wyœwietlony b³¹d: " + actualErrorMessage);
            Assert.IsTrue(actualErrorMessage.Contains("Kod bezpieczeñstwa karty jest niekompletny."), "B³¹d kodu CVC karty nie zosta³ wyœwietlony");

            FillOutCardData("4000000000000002", "0227", "456");
            driver.FindElement(orderButton).Click();
            actualErrorMessage = WaitForErrorMessage();
            log.Info("Wyœwietlony b³¹d: " + actualErrorMessage);

            Assert.IsTrue(actualErrorMessage.Contains("Karta zosta³a odrzucona."), "B³¹d o odrzuceniu p³atnoœci nie zosta³ wyœwietlony");

            FillOutCardData("4242424242424242", "0227", "456");
            OrderAndWaitToComplete();

            int numberOfOrderReceivedMessages = driver.FindElements(By.CssSelector(".woocommerce-thankyou-order-received")).Count;
            Assert.AreEqual(1, numberOfOrderReceivedMessages, "Nieprawid³owy komunikat o otrzymaniu zamówienia, czy p³atnoœæ zosta³a poprawnie przetworzona?");
        }

        [Test]
        public void OrderHistoryTest()
        {
            WebDriverWait Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            AddProductToCart("http://zelektronika.store/product/komputer");
            ViewCart();
            driver.FindElement(checkoutButton).Click();

            LogInDuringCheckout("dmolewskisklep", "testowekontoztestowymhaslem");
            FillOutCardData("4242424242424242", "0226", "456");

            CheckConfirmationBox();
            int orderNumber = int.Parse(OrderAndWaitToComplete());

            GoToMyAccountOrders();

            int numberOfOrdersWithGivenNumber = driver.FindElements(By.XPath("//a[contains(text(), '#" + orderNumber + "')]")).Count;

            Assert.AreEqual(1, numberOfOrdersWithGivenNumber, "Expected one order with a given number (" + orderNumber + ") but found " + numberOfOrdersWithGivenNumber + " orders.");

            Wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.CssSelector("table.shop_table")));

            IList<IWebElement> orders = driver.FindElements(By.CssSelector("table.shop_table tr.order"));
            log.Info("W tabeli wyœwietlono (" + orders.Count + ") ostatnich zamówieñ");

            Assert.Multiple(() =>
            {
                foreach (IWebElement order in orders)
                {
                    string orderNumberTable = order.FindElement(By.CssSelector("td.woocommerce-orders-table__cell.woocommerce-orders-table__cell-order-number")).Text;
                    string orderDate = order.FindElement(By.CssSelector("td.woocommerce-orders-table__cell.woocommerce-orders-table__cell-order-date")).Text;
                    string orderTotal = order.FindElement(By.CssSelector("td.woocommerce-orders-table__cell.woocommerce-orders-table__cell-order-total")).Text;

                    Assert.IsNotNull(orderNumberTable, "Pole nr zamówienia jest puste dla zamówienia z dat¹: " + orderDate);
                    Assert.IsNotNull(orderDate, "Pole data jest puste dla zamówienia: " + orderNumber);
                    Assert.IsNotNull(orderTotal, "Pole kwota jest puste dla zamówienia: " + orderNumber);
                }

                log.Info("Wszystkie zamówienia posiadaj¹ niepuste pole numer");
                log.Info("Wszystkie zamówienia posiadaj¹ niepuste pole data");
                log.Info("Wszystkie zamówienia posiadaj¹ niepuste pole z kwot¹ zamówienia");
            });

            IWebElement viewButton = driver.FindElement(By.CssSelector("td.woocommerce-orders-table__cell.woocommerce-orders-table__cell-order-actions a.woocommerce-button.wp-element-button.button.view"));
            string href = viewButton.GetAttribute("href");
            viewButton.Click();

            log.Info("Podsumowanie zamówienia nr " + orderNumber);
            log.Info("Link do podsumowania ostatniego zamówienia: " + href);

            string[] parts = href.Split('/');
            string actualOrderNumber = parts[parts.Length - 2];

            if (!string.IsNullOrEmpty(actualOrderNumber) && actualOrderNumber.All(char.IsDigit))
            {
                int actualOrderNumberInt = int.Parse(actualOrderNumber);
                Assert.AreEqual(actualOrderNumberInt, orderNumber, "Numer zamówienia z linku z tabeli nie jest równy temu ze z³o¿onego w tescie zamówienia");
            }
            else
            {
                Assert.Fail("Niepoprawny numer zamówienia z linku");
            }

            //Assert.AreEqual(actualOrderNumberInt, orderNumber, "Numer zamówienia z linku z tabeli nie jest równy temu ze z³o¿onego w tescie zamówienia");
            Assert.AreEqual(GetAccountMessage(), "Zamówienie nr " + orderNumber + " z³o¿one " + GetCurrentDate() + " jest obecnie W trakcie realizacji.", "Numer zamówienia z linku z tabeli nie jest równy temu ze z³o¿onego w tescie zamówienia");

            CheckOrderDetailsSection();
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
            WebDriverWait Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            try
            {
                Wait.Until(d => driver.FindElements(by).Count == 0);
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("Elements located by " + by + " didn't disappear in 5 seconds.");
                throw;
            }
        }
    }

}