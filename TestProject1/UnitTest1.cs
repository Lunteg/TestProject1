using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace AutomatedTests
{
    public class Tests
    {
        IWebDriver driver;

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            // развернуть окно браузера
            driver.Manage().Window.Maximize();
            // неявное ожидание
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(7);
            driver.Navigate().GoToUrl("https://www.richersounds.com/");
            driver.FindElement(By.XPath("//button[@id='onetrust-accept-btn-handler']")).Click();
            driver.FindElement(By.XPath("//button[@class='action-close']")).Click();
        }

        [Test]
        public void TestPriceFilter()
        {
            driver.FindElement(By.XPath("//a[@id='ui-id-3']")).Click();
            driver.FindElement(By.XPath("//img[@title='Audio recorders']")).Click();


            // левый и правый слайдер цен
            IWebElement left_slider = driver.FindElement(By.XPath("//a[@class='ui-slider-handle ui-state-default ui-corner-all']"));
            IWebElement right_slider = driver.FindElement(By.XPath("//div[@id='slider-range']/a[2]"));

            Actions action = new Actions(driver);
            action.Click();

            int xCoord = left_slider.Location.X;

            //двигаем ползуночек (фильтр цен)
            action.DragAndDropToOffset(left_slider, 50, 0);
            action.DragAndDropToOffset(right_slider, -30, 0);
            action.Build().Perform();

            // применяем фильтр цен
            driver.FindElement(By.XPath("//button[@data-role='aw-layered-nav-price-submit']")).Click();

            //прикол для того, чтобы считать строку со знаком фунта
            CultureInfo provider = new CultureInfo("en-GB");
            NumberStyles style = NumberStyles.Currency | NumberStyles.AllowCurrencySymbol;


            new WebDriverWait(driver, TimeSpan.FromSeconds(3))
                .Until(x => driver.FindElements(By.XPath("//span[@data-role='aw-layered-nav-price-label-from']")).Any());
            new WebDriverWait(driver, TimeSpan.FromSeconds(3))
                .Until(x => driver.FindElements(By.XPath("//span[@data-role='aw-layered-nav-price-label-to']")).Any());
            //Console.WriteLine(driver.FindElement(By.XPath("//span[@data-role='aw-layered-nav-price-label-to']")).Text.Trim());
            //кастуем в инт
            int minCost_v = int.Parse(driver.FindElement(By.XPath("//span[@data-role='aw-layered-nav-price-label-from']")).Text.Trim(), style, provider);
            int maxCost_v = int.Parse(driver.FindElement(By.XPath("//span[@data-role='aw-layered-nav-price-label-to']")).Text.Trim(), style, provider);
            //Console.WriteLine(maxCost_v);
            //Console.WriteLine(maxCost_v);

            //Console.WriteLine(double.Parse(driver.FindElement(By.XPath("//div[@class='price-box price-final_price']//span[@class='price']")).Text, style, provider));

            double[] actualValues = Array.ConvertAll(driver.FindElements(By.XPath("//div[@class='price-box price-final_price']//span[@class='price']"))
               .Select(webPrice => webPrice.Text).ToArray<string>(), s => double.Parse(s, style, provider));
            actualValues.ToList().ForEach(actualPrice => Assert.True(actualPrice >= minCost_v && actualPrice <= maxCost_v, "Price filter works wrong. Actual price is " + actualPrice + ". But should be more or equal than" + 500 + "and less or equal than" + 300));

        }
        [Test]
        public void TestTooltipText()
        {
            driver.FindElement(By.XPath("//a[@id='ui-id-4']")).Click();
            driver.FindElement(By.XPath("//img[@title='HDMI Cables']")).Click();
            driver.FindElement(By.XPath("//div[@class='products wrapper grid products-grid']//img[@class='product-image-photo']")).Click();

            new WebDriverWait(driver, TimeSpan.FromSeconds(3))
                .Until(x => driver.FindElements(By.XPath("//button[@title='Add to Cart']")).Any());
            //проверяем появилось ли значение, указанное в  title
            Assert.AreEqual("Add to Cart", driver.FindElement(By.XPath("//button[@title='Add to Cart']")).GetAttribute("title"), "Tooltip has not appeared.");
        }
        [Test]
        public void NegativeSignUpTest()
        {
            // заходим в соответствующий раздел на сайте
            driver.FindElement(By.XPath("//a[@class='authorization-link header-icon']")).Click();
            //переходим к регестрации
            driver.FindElement(By.XPath("//div[@class='item title left']")).Click();
            driver.FindElement(By.Id("telephone")).SendKeys("+79841465720");
            driver.FindElement(By.Id("age-check")).Click();
            //Console.WriteLine( driver.FindElement(By.Id("age-check")).Selected);
            driver.FindElement(By.Id("email_address")).SendKeys("vfbdhjsk57bs442@mail.ru");
            driver.FindElement(By.Id("password")).SendKeys("zK%N12Qb");

            //пробуем создать аккаунт
            driver.FindElement(By.XPath("//button[@title='Create an Account']")).Click();
            // проверяем, что выводится ошибка
            Assert.AreEqual("This is a required field.", driver.FindElement(By.Id("password-confirmation-error")).Text,
                "registration is allowed in the absence of password confirmation.");

        }

        [TearDown]
        public void CleanUp()
        {
            driver.Quit();
        }

    }
}