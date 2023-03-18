using Allure.Commons;
using NUnit.Allure.Attributes;
using NUnit.Allure.Core;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools.V108.Accessibility;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SeleniumTests
{
    [AllureNUnit]
    class StoreTests

    {
        IWebDriver driver;
        private WebDriverWait wait;

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

        [OneTimeSetUp]
        public void Setup()
        {
            FirefoxOptions options = new FirefoxOptions();
            //options.AddArgument("--headless");
            FirefoxProfile profile = new FirefoxProfile();
            profile.SetPreference("extensions.webextensions.enabled", false);
            options.Profile = profile;

            driver = new FirefoxDriver(options);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);
            driver.Manage().Window.Maximize();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            js = (IJavaScriptExecutor)driver;
            driver.Navigate().GoToUrl(storeURL);
        }

        [TearDown]
        public void ClearCache()
        {
            if (TestContext.CurrentContext.Result.Outcome != ResultState.Success)
            {
                string screenshotPath = TakeScreenshot();
                Console.WriteLine("Screenshot: " + screenshotPath);
                AllureLifecycle.Instance.AddAttachment(screenshotPath);
            }
            if (driver != null)
            {
                driver.Manage().Cookies.DeleteAllCookies();
            }
        }

        [OneTimeTearDown]
        public void QuitDriver()
        {
            driver.Quit();
        }
        private static string GetCurrentDate()
        {
            return DateTime.Now.ToString("d MMMM yyyy");
        }

        private void SwitchToFrame(By frameLocator)
        {
            wait.Until(ExpectedConditions.FrameToBeAvailableAndSwitchToIt(frameLocator));
            wait.Until(d => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
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
            Console.WriteLine($"Znaleziony tekst: \"{subpageContent}\", oczekiwany: \"{expectedText}\"");
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

            //searchBox.SendKeys(productName, Keys.RETURN); //tylko dla Chrome, Firefox wymusza u�ycie klasy Actions w przypadku u�ycia klawisza ENTER/RETURN
        }

        private void SortByPriceLowToHigh()
        {
            IWebElement sortByDropdown = driver.FindElement(By.CssSelector(".woocommerce-ordering select"));
            sortByDropdown.Click();

            SelectElement select = new SelectElement(sortByDropdown);
            select.SelectByText("Sortuj po cenie od najni�szej");
        }

        private List<double> GetProductPrices()
        {
            IList<IWebElement> productPriceElements = driver.FindElements(By.CssSelector(".price"));
            List<double> productPrices = new List<double>();
            foreach (IWebElement productPriceElement in productPriceElements)
            {
                string productPriceText = productPriceElement.Text.Replace(",", ".").Replace("z�", "").Trim();
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

            Assert.IsTrue(WaitForMessage().Contains("�" + productName + "� zosta� dodany do koszyka."), "Produkt �" + productName + "� nie zosta� dodany do koszyka");
            Console.WriteLine("Dodano nowy produkt do koszyka: �" + productName + "�");
        }

        private string WaitForMessage()
        {
            wait.Until(d => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
            var messageElements = wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector(".woocommerce-message")));
            var messages = messageElements.Select(e => e.Text);
            return string.Join(Environment.NewLine, messages);
            //return wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector(".woocommerce-message"))).Text;
        }

        private string WaitForErrorMessage()
        {
            By errorList = By.CssSelector("ul.woocommerce-error");
            wait.Until(d => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
            return wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(errorList))[0].Text;
        }

        private double AddProductPrice()
        {
            string productPriceText = driver.FindElement(By.CssSelector("div.summary.entry-summary bdi:nth-child(1)")).Text;
            double productPrice = double.Parse(Regex.Replace(productPriceText, @"[^0-9.,]+", "").Replace(",", "."), CultureInfo.InvariantCulture);
            wait.Until(ExpectedConditions.ElementExists(By.CssSelector("div#content p.woocommerce-mini-cart__total.total bdi:nth-child(1)")));
            return productPrice;
        }

        private void ViewCart()
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(ProductPageViewCartButton)).Click();
            wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(ShopTable));
        }

        private double GetCartTotalPrice()
        {
            string cartTotalPriceText;
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(".blockOverlay")));
            wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("tr[class='cart-subtotal'] bdi:nth-child(1)")));
            cartTotalPriceText = driver.FindElement(By.CssSelector("tr[class='cart-subtotal'] bdi:nth-child(1)")).Text.Replace(",", ".");
            return double.Parse(cartTotalPriceText.Replace("z�", ""), CultureInfo.InvariantCulture);
        }

        private double GetCartTotalPriceWithCoupon()
        {
            string cartTotalPriceText;
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(".blockOverlay")));
            wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("tr[class='order-total'] bdi:nth-child(1)")));
            cartTotalPriceText = driver.FindElement(By.CssSelector("tr[class='order-total'] bdi:nth-child(1)")).Text.Replace(",", ".");
            return double.Parse(cartTotalPriceText.Replace("z�", ""), CultureInfo.InvariantCulture);
        }

        private void AddCoupon()
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("#coupon_code"))).SendKeys("rabatwsti");
            wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button[value='Wykorzystaj kupon']"))).Click();

            string couponMessage = WaitForMessage();

            string expectedCouponMessage = "Kupon zosta� pomy�lnie u�yty.";
            Assert.AreEqual(expectedCouponMessage, couponMessage, "Kupon nie zosta� dodany do koszyka");

            Console.WriteLine("Dodano kupon rabatowy");
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(".blockOverlay")));

            IWebElement freeShippingAvailablility = driver.FindElement(By.CssSelector("label[for='shipping_method_0_free_shipping1']"));
            Assert.IsNotNull(freeShippingAvailablility, "Darmowa dostawa jest nie dost�pna");
        }

        public void RemoveProductFromCart()
        {
            driver.FindElement(RemoveProductButton).Click();
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(".blockOverlay")));
        }

        private void FillOutCheckoutForm(string email, string phone)
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(firstNameField)).Clear();
            wait.Until(ExpectedConditions.ElementToBeClickable(firstNameField)).SendKeys("Helena");
            wait.Until(ExpectedConditions.ElementToBeClickable(lastNameField)).Clear();
            wait.Until(ExpectedConditions.ElementToBeClickable(lastNameField)).SendKeys("Mazur");
            wait.Until(ExpectedConditions.ElementToBeClickable(countryCodeArrow)).Click();
            wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("li[id*='-PL']"))).Click();
            wait.Until(ExpectedConditions.ElementToBeClickable(addressField)).Click();
            wait.Until(ExpectedConditions.ElementToBeClickable(addressField)).Clear();
            wait.Until(ExpectedConditions.ElementToBeClickable(addressField)).SendKeys("Diamentowa 145");
            wait.Until(ExpectedConditions.ElementToBeClickable(postalCodeField)).Click();
            wait.Until(ExpectedConditions.ElementToBeClickable(postalCodeField)).Clear();
            wait.Until(ExpectedConditions.ElementToBeClickable(postalCodeField)).SendKeys("71-232");
            wait.Until(ExpectedConditions.ElementToBeClickable(cityField)).Clear();
            wait.Until(ExpectedConditions.ElementToBeClickable(cityField)).SendKeys("Szczecin");
            wait.Until(ExpectedConditions.ElementToBeClickable(phoneField)).Clear();
            wait.Until(ExpectedConditions.ElementToBeClickable(phoneField)).SendKeys(phone);
            wait.Until(ExpectedConditions.ElementToBeClickable(emailField)).Clear();
            wait.Until(ExpectedConditions.ElementToBeClickable(emailField)).SendKeys(email);
        }

        private void FillOutCardData(string cardNumber, string expirationDate, string cvc)
        {
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(".blockOverlay")));

            //zamiast zwyk�ego findElement dla wi�kszej stabilno�ci test�w w FireFox
            IWebElement element = driver.FindElement(shippingMethod);
            IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
            executor.ExecuteScript("arguments[0].click();", element);

            //driver.FindElement(shippingMethod).click();

            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(".blockOverlay")));

            driver.FindElement(paymentMethod).Click();
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(".blockOverlay")));

            SwitchToFrame(cardNumberFrame);
            IWebElement cardNumberElement = wait.Until(ExpectedConditions.ElementToBeClickable(cardNumberField));
            cardNumberElement.Clear();
            SlowType(cardNumberElement, cardNumber);
            driver.SwitchTo().DefaultContent();

            SwitchToFrame(expirationDateFrame);
            IWebElement expirationDateElement = wait.Until(ExpectedConditions.ElementToBeClickable(expirationDateField));
            expirationDateElement.Clear();
            SlowType(expirationDateElement, expirationDate);
            driver.SwitchTo().DefaultContent();

            SwitchToFrame(cvcFrame);
            IWebElement cvcElement = wait.Until(ExpectedConditions.ElementToBeClickable(cvcField));
            cvcElement.Clear();
            SlowType(cvcElement, cvc);
            driver.SwitchTo().DefaultContent();
        }

        private void CheckConfirmationBox()
        {
            driver.SwitchTo().DefaultContent();
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(".blockOverlay")));
            IWebElement confirmationBox = driver.FindElement(By.CssSelector("input#terms"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", confirmationBox);
            confirmationBox.Click();
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(".blockOverlay")));
        }

        private string OrderAndWaitToComplete()
        {
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(".blockOverlay")));
            driver.FindElement(orderButton).Click();

            wait.Until(ExpectedConditions.UrlContains("/checkout/zamowienie-otrzymane/"));
            string orderNumber = driver.FindElement(By.CssSelector(".order>strong")).Text;
            Console.WriteLine("Zam�wienie z�o�one poprawnie - nr zam�wienia: " + orderNumber);
            return orderNumber;
        }

        private void LogInDuringCheckout(string userName, string password)
        {
            By expandLoginForm = By.CssSelector(".showlogin");
            By wrappedLoginView = By.CssSelector(".login[style='display: none;']");

            By usernameField = By.CssSelector("#username");
            By passwordField = By.CssSelector("#password");
            By loginButton = By.CssSelector("[name='login']");

            wait.Until(ExpectedConditions.ElementToBeClickable(expandLoginForm)).Click();
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(wrappedLoginView));
            wait.Until(ExpectedConditions.ElementToBeClickable(usernameField)).SendKeys(userName);
            wait.Until(ExpectedConditions.ElementToBeClickable(passwordField)).SendKeys(password);
            driver.FindElement(loginButton).Click();
            Console.WriteLine("Zalogowano z nazw� u�ytkownika: " + userName);
        }

        private void GoToMyAccountOrders()
        {
            driver.FindElement(By.CssSelector("li[id='menu-item-19']")).Click();
            wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".woocommerce-MyAccount-navigation-link--orders"))).Click();
        }

        private void CheckOrderDetailsSection()
        {
            wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.CssSelector("section.woocommerce-order-details")));
            IWebElement orderDetailsSection = driver.FindElement(By.CssSelector("section.woocommerce-order-details"));

            IList<IWebElement> orderDetailsLabels = orderDetailsSection.FindElements(By.CssSelector("th"));
            IList<IWebElement> orderDetailsValues = orderDetailsSection.FindElements(By.CssSelector("td"));

            foreach (IWebElement label in orderDetailsLabels)
            {
                string labelText = label.Text.Trim();
                int index = orderDetailsLabels.IndexOf(label);
                string valueText = orderDetailsValues[index].Text.Trim();
                Console.WriteLine(labelText + " " + valueText);
                Assert.IsNotNull(valueText, "Warto�� pola: \"" + labelText + "\" jest pusta");
            }

            IWebElement billingAddressSection = driver.FindElement(By.CssSelector("div.woocommerce-column--billing-address"));
            IWebElement billingAddressElement = billingAddressSection.FindElement(By.CssSelector("address"));
            Assert.IsNotNull(billingAddressElement, "Adres rozliczeniowy jest pusty");
            Console.WriteLine(billingAddressSection.Text);

            IWebElement shippingAddressSection = driver.FindElement(By.CssSelector("div.woocommerce-column--shipping-address"));
            IWebElement shippingAddressElement = shippingAddressSection.FindElement(By.CssSelector("address"));
            Console.WriteLine(shippingAddressSection.Text);
            Assert.IsNotNull(shippingAddressElement, "Adres do wysy�ki jest pusty");
        }

        private void VerifySearchResults(string searchTerm)
        {
            wait.Until(ExpectedConditions.UrlContains(searchTerm));
            wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector(".product")));

            IList<IWebElement> searchResultElements = driver.FindElements(By.CssSelector(".product"));
            int numSearchResults = searchResultElements.Count;

            Assert.IsTrue(numSearchResults > 0, "Brak wynik�w wyszukiwania dla zadanego zapytania");

            Console.WriteLine($"Liczba wynik�w wyszukiwania: {numSearchResults}");

            foreach (IWebElement searchResultElement in searchResultElements)
            {
                string searchResultText = searchResultElement.Text;
                Assert.IsTrue(searchResultText.ToLower().Contains(searchTerm), $"Wynik wyszukiwania nie zawiera frazy: \"{searchTerm}\"");
            }
            Console.WriteLine($"Wszystkie wyniki wyszukiwania zawieraj� fraz� \"{searchTerm}\"");
        }

        [Test, Order(1)]
        [AllureName("Test rejestracji z prawid�owymi danymi")]
        [Description("Przypadek testowy dotyczy wymagania pierwszego." +
            "\nRejestracja i logowanie: Klienci powinni mie� mo�liwo�� zarejestrowania si� i zalogowania na swoje konto, aby mie� dost�p do swoich danych i historii zakup�w.")]
        public void RegisterWithEmailAndPassword()
        {
            string email = username + randomNumber + "@gmail.com";
            Register(email, password);

            string myAccountContent = GetAccountMessage();
            string expectedName = userFullName + randomNumber;
            string errorMessage = string.Format("Strona nie zawiera spodziewanej nazwy u�ytkownika: \"{0}\", znaleziono: \"{1}\"", expectedName, myAccountContent);
            Assert.IsTrue(myAccountContent.Contains(expectedName), errorMessage);
            Console.WriteLine(string.Format("Oczekiwana nazwa u�ytkownika: \"{0}\", strona zawiera nazw� u�ytkownika: \"{1}\"", expectedName, myAccountContent));

            string ordersSelector = ".woocommerce-MyAccount-navigation-link--orders";
            string editAddressSelector = ".woocommerce-MyAccount-navigation-link--edit-address";
            string paymentMethodsSelector = ".woocommerce-MyAccount-navigation-link--payment-methods";
            Assert.IsTrue(driver.FindElement(By.CssSelector(ordersSelector)).Text.Contains("Zam�wienia"), "Strona nie zawiera spodziewanego przycisku: Zam�wienia");
            Assert.IsTrue(driver.FindElement(By.CssSelector(editAddressSelector)).Text.Contains("Adresy"), "Strona nie zawiera spodziewanego przycisku: Adresy");
            Assert.IsTrue(driver.FindElement(By.CssSelector(paymentMethodsSelector)).Text.Contains("Metody p�atno�ci"), "Strona nie zawiera spodziewanego przycisku: Metody p�atno�ci");

            GoToMyAccountSubpage(editAddressSelector, "adresy");
            GoToMyAccountSubpage(paymentMethodsSelector, "metod");

            DeleteAccount();
            driver.Navigate().GoToUrl(storeURL);
        }

        [Test, Order(2)]
        [AllureName("Test wyszukiwania produkt�w i sortowania wynik�w")]
        [Description("Przypadek testowy dotyczy wymagania drugiego." +
            "\nKatalog produkt�w: Sklep powinien posiada� wygodny i przejrzysty katalog produkt�w, kt�ry umo�liwia �atwe wyszukiwanie i sortowanie produkt�w.")]
        public void SearchInStoreAndSortResults()
        {
            string productName = "komputer";
            SearchForProduct(productName);
            VerifySearchResults(productName);

            SortByPriceLowToHigh();
            List<double> productPrices = GetProductPrices();

            Console.WriteLine($"Na stronie wynik�w s� widoczne ({productPrices.Count}) ceny produkt�w");
            Console.WriteLine($"Ceny produkt�w: {string.Join(", ", productPrices)}");

            List<double> sortedPrices = new List<double>(productPrices);
            sortedPrices.Sort();
            Assert.AreEqual(sortedPrices, productPrices, "Produkty nie s� posortowane od najni�szej ceny");
            Console.WriteLine($"Posortowane ceny produkt�w: {string.Join(", ", sortedPrices)}");
        }

        [Test, Order(3)]
        [AllureName("Test szybkiego dodawania produkt�w do koszyka")]
        [Description("Przypadek testowy dotyczy wymagania trzeciego i czwartego." +
            "\nSzybkie dodawanie do koszyka: Klienci powinni mie� mo�liwo�� szybkiego dodawania produkt�w do koszyka za pomoc� jednego klikni�cia." +
            "\nKoszyk: Klienci powinni mie� mo�liwo�� przegl�dania i modyfikowania produkt�w w swoim koszyku, dodania kodu rabatowego a tak�e przej�cia do procesu zam�wienia.")]
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
                Console.WriteLine($"Aktualna warto�� koszyka: {totalPrice}");
            }

            ViewCart();
            double cartTotalPrice = GetCartTotalPrice();
            Assert.AreEqual(cartTotalPrice, totalPrice, 0.02, $"Cena produkt�w w koszyku ({GetCartTotalPrice()}) nie jest r�wna obliczonej w te�cie: {totalPrice}");

            int numberOfItems = driver.FindElements(By.CssSelector(".cart_item")).Count;
            Assert.AreEqual(productPages.Length, numberOfItems, $"Ilo�� produkt�w w koszyku jest nieprawid�owa. Wymagane: {productPages.Length}, w ramach testu obliczono: {numberOfItems}");

            AddCoupon();

            double cartTotalPriceAfterCoupon = GetCartTotalPriceWithCoupon();
            Console.WriteLine($"Cena po rabacie: {cartTotalPriceAfterCoupon}");
            Console.WriteLine($"Cena po rabacie: {cartTotalPriceAfterCoupon}");

            double expectedDiscountedPrice = cartTotalPrice * 0.8;
            Assert.AreEqual(cartTotalPriceAfterCoupon, expectedDiscountedPrice, 0.02, $"Cena po uwzgl�dnieniu rabatu nie jest r�wna oczekiwanej: {cartTotalPriceAfterCoupon} =/= {expectedDiscountedPrice}");

            RemoveProductFromCart();
            numberOfItems = driver.FindElements(By.CssSelector(".cart_item")).Count;
            Assert.AreEqual(productPages.Length - 1, numberOfItems, $"Ilo�� produkt�w w koszyku jest nieprawid�owa. Wymagane: {productPages.Length}, w ramach testu obliczono: {numberOfItems}");
        }

        [Test, Order(4)]
        [AllureName("Test wype�niania formularza i sk�adania zam�wienia")]
        [Description("Przypadek testowy dotyczy wymagania pi�tego oraz si�dmego." +
            "\nWype�nianie formularza zam�wienia: Klienci powinni mie� mo�liwo�� wype�nienia formularza zam�wienia, w kt�rym b�d� wprowadza� swoje dane i informacje dotycz�ce dostawy." +
            "\nPotwierdzenie zam�wienia: Po zako�czeniu procesu zam�wienia, klienci powinni otrzyma� potwierdzenie zam�wienia.")]
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
            Assert.AreEqual(expectedNumberOfMessages, numberOfOrderReceivedMessages, "Nieprawid�owy komunikat o otrzymaniu zam�wienia, czy p�atno�� zosta�a poprawnie przetworzona?");

            string dateFromSummary = driver.FindElement(summaryDate).Text;
            string currentDate = GetCurrentDate();
            string actualPrice = driver.FindElement(summaryPrice).Text;
            string expectedPrice = "2008,99 z�";
            string actualPaymentMethod = driver.FindElement(summaryPaymentMethod).Text;
            string expectedPaymentMethod = "Karta p�atnicza (Stripe)";
            int actualNumberOfProducts = driver.FindElements(summaryProductRows).Count;
            int expectedNumberOfProducts = 1;
            string actualProductQuantity = driver.FindElement(summaryProductQuantity).Text;
            string expectedProductQuantity = "� 1";
            string actualProductName = driver.FindElement(summaryProductName).Text;
            string expectedProductName = "Komputer";

            Assert.Multiple(() =>
            {
                Assert.IsTrue(orderNumber > 0, "Numer zam�wienia nie jest wi�kszy ni� 0");
                Assert.AreEqual(currentDate, dateFromSummary, $"Data w podsumowaniu nieprawid�owa. Oczekiwana: {currentDate}, w podsumowaniu: {dateFromSummary}");
                Assert.AreEqual(expectedPrice, actualPrice, $"Cena w podsumowaniu nieprawid�owa. Oczekiwana: {expectedPrice}, w podsumowaniu: {actualPrice}");
                Assert.AreEqual(expectedPaymentMethod, actualPaymentMethod, $"Metoda p�atno�ci w podsumowaniu nieprawid�owa. Oczekiwana: {expectedPaymentMethod} w podsumowaniu: {actualPaymentMethod}");
                Assert.AreEqual(expectedNumberOfProducts, actualNumberOfProducts, $"Produkty w podsumowaniu nieprawid�owe. Oczekiwane: {expectedNumberOfProducts} w podsumowaniu: {actualNumberOfProducts}");
                Assert.AreEqual(expectedProductQuantity, actualProductQuantity, $"Liczba produkt�w w podsumowaniu nieprawid�owa. Oczekiwana: {expectedProductQuantity} w podsumowaniu: {actualProductQuantity}");
                Assert.AreEqual(expectedProductName, actualProductName, $"Nazwa produktu w podsumowaniu nieprawid�owa. Oczekiwana: {expectedProductName} w podsumowaniu: {actualProductName}");
                Console.WriteLine("Dane w podsumowaniu zam�wienia s� poprawne");
            });
        }

        [Test, Order(5)]
        [AllureName("Test modu�u p�atno�ci, weryfikacji i autoryzacji p�atno�ci testowymi kartami")]
        [Description("Przypadek testowy dotyczy wymagania sz�stego." +
            "\nWeryfikacja p�atno�ci: Sklep powinien umo�liwia� weryfikacj� i autoryzacj� p�atno�ci przed dokonaniem transakcji.")]
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
            Console.WriteLine("Wy�wietlony b��d: " + actualErrorMessage);
            Assert.IsTrue(actualErrorMessage.Contains("Data wa�no�ci karty ju� min�a."), "B��d daty wa�no�ci karty nie zosta� wy�wietlony");

            FillOutCardData("4000000000000002", "0227", "");
            driver.FindElement(orderButton).Click();
            actualErrorMessage = WaitForErrorMessage();
            Console.WriteLine("Wy�wietlony b��d: " + actualErrorMessage);
            Assert.IsTrue(actualErrorMessage.Contains("Kod bezpiecze�stwa karty jest niekompletny."), "B��d kodu CVC karty nie zosta� wy�wietlony");

            FillOutCardData("4000000000000002", "0227", "456");
            driver.FindElement(orderButton).Click();
            actualErrorMessage = WaitForErrorMessage();
            Console.WriteLine("Wy�wietlony b��d: " + actualErrorMessage);

            Assert.IsTrue(actualErrorMessage.Contains("Karta zosta�a odrzucona."), "B��d o odrzuceniu p�atno�ci nie zosta� wy�wietlony");

            FillOutCardData("4242424242424242", "0227", "456");
            OrderAndWaitToComplete();

            int numberOfOrderReceivedMessages = driver.FindElements(By.CssSelector(".woocommerce-thankyou-order-received")).Count;
            Assert.AreEqual(1, numberOfOrderReceivedMessages, "Nieprawid�owy komunikat o otrzymaniu zam�wienia, czy p�atno�� zosta�a poprawnie przetworzona?");
        }

        [Test, Order(6)]
        [Description("Test historii i szczeg��w zam�wie�")]
        [AllureName("Przypadek testowy dotyczy wymagania �smego." +
            "\nHistoria zam�wie�: Klienci powinni mie� dost�p do swojej historii zam�wie� i szczeg��w dotycz�cych ka�dego z nich.")]
        public void OrderHistoryTest()
        {
            AddProductToCart("http://zelektronika.store/product/komputer");
            ViewCart();
            driver.FindElement(checkoutButton).Click();

            LogInDuringCheckout("dmolewskisklep", "testowekontoztestowymhaslem");
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(".blockOverlay")));
            FillOutCardData("4242424242424242", "0226", "456");

            CheckConfirmationBox();
            int orderNumber = int.Parse(OrderAndWaitToComplete());

            GoToMyAccountOrders();

            int numberOfOrdersWithGivenNumber = driver.FindElements(By.XPath("//a[contains(text(), '#" + orderNumber + "')]")).Count;

            Assert.AreEqual(1, numberOfOrdersWithGivenNumber, "Expected one order with a given number (" + orderNumber + ") but found " + numberOfOrdersWithGivenNumber + " orders.");

            wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.CssSelector("table.shop_table")));

            IList<IWebElement> orders = driver.FindElements(By.CssSelector("table.shop_table tr.order"));
            Console.WriteLine("W tabeli wy�wietlono (" + orders.Count + ") ostatnich zam�wie�");

            Assert.Multiple(() =>
            {
                foreach (IWebElement order in orders)
                {
                    string orderNumberTable = order.FindElement(By.CssSelector("td.woocommerce-orders-table__cell.woocommerce-orders-table__cell-order-number")).Text;
                    string orderDate = order.FindElement(By.CssSelector("td.woocommerce-orders-table__cell.woocommerce-orders-table__cell-order-date")).Text;
                    string orderTotal = order.FindElement(By.CssSelector("td.woocommerce-orders-table__cell.woocommerce-orders-table__cell-order-total")).Text;

                    Assert.IsNotNull(orderNumberTable, "Pole nr zam�wienia jest puste dla zam�wienia z dat�: " + orderDate);
                    Assert.IsNotNull(orderDate, "Pole data jest puste dla zam�wienia: " + orderNumber);
                    Assert.IsNotNull(orderTotal, "Pole kwota jest puste dla zam�wienia: " + orderNumber);
                }

                Console.WriteLine("Wszystkie zam�wienia posiadaj� niepuste pole numer");
                Console.WriteLine("Wszystkie zam�wienia posiadaj� niepuste pole data");
                Console.WriteLine("Wszystkie zam�wienia posiadaj� niepuste pole z kwot� zam�wienia");
            });

            IWebElement viewButton = driver.FindElement(By.CssSelector("td.woocommerce-orders-table__cell.woocommerce-orders-table__cell-order-actions a.woocommerce-button.wp-element-button.button.view"));
            string href = viewButton.GetAttribute("href");
            viewButton.Click();

            Console.WriteLine("Podsumowanie zam�wienia nr " + orderNumber);
            Console.WriteLine("Link do podsumowania ostatniego zam�wienia: " + href);

            string[] parts = href.Split('/');
            string actualOrderNumber = parts[parts.Length - 2];

            if (!string.IsNullOrEmpty(actualOrderNumber) && actualOrderNumber.All(char.IsDigit))
            {
                int actualOrderNumberInt = int.Parse(actualOrderNumber);
                Assert.AreEqual(actualOrderNumberInt, orderNumber, "Numer zam�wienia z linku z tabeli nie jest r�wny temu ze z�o�onego w tescie zam�wienia");
            }
            else
            {
                Assert.Fail("Niepoprawny numer zam�wienia z linku");
            }

            //Assert.AreEqual(actualOrderNumberInt, orderNumber, "Numer zam�wienia z linku z tabeli nie jest r�wny temu ze z�o�onego w tescie zam�wienia");
            Assert.AreEqual(GetAccountMessage(), "Zam�wienie nr " + orderNumber + " z�o�one " + GetCurrentDate() + " jest obecnie W trakcie realizacji.", "Numer zam�wienia z linku z tabeli nie jest r�wny temu ze z�o�onego w tescie zam�wienia");

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