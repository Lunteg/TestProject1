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
    public class RicherSoundsTests
    {
        IWebDriver driver;

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(7);
            driver.Navigate().GoToUrl("https://www.richersounds.com/");
            driver.FindElement(By.XPath("//button[@id='onetrust-accept-btn-handler']")).Click();

            IWebElement element = driver.FindElement(By.XPath("//div[@class='wrapper']"));
            Actions actionProvider = new Actions(driver);
            actionProvider.MoveByOffset(10, 20).Click().Build().Perform();
        }

        [Test]
        public void TestPriceFilter()
        {
            driver.FindElement(By.XPath("//a[@href='https://www.richersounds.com/hi-fi.html' and @role = 'menuitem']")).Click();
            driver.FindElement(By.XPath("//img[@title='Audio recorders']")).Click();


            // левый и правый слайдер цен
            IWebElement left_slider = driver.FindElement(By.XPath("//div[@id='slider-range']/a[1]"));
            IWebElement right_slider = driver.FindElement(By.XPath("//div[@id='slider-range']/a[2]"));

            Actions action = new Actions(driver);
            action.Click();

            int xCoord = left_slider.Location.X;

            action.DragAndDropToOffset(left_slider, 50, 0);
            action.DragAndDropToOffset(right_slider, -30, 0);
            action.Build().Perform();

            driver.FindElement(By.XPath("//button[@data-role='aw-layered-nav-price-submit']")).Click();

            CultureInfo provider = new CultureInfo("en-GB");
            NumberStyles style = NumberStyles.Currency | NumberStyles.AllowCurrencySymbol;


            driver.FindElements(By.XPath("//span[@data-role='aw-layered-nav-price-label-from']"));
            driver.FindElements(By.XPath("//span[@data-role='aw-layered-nav-price-label-to']"));

            //кастуем в инт
            int minCost = int.Parse(driver.FindElement(By.XPath("//span[@data-role='aw-layered-nav-price-label-from']")).Text.Trim(), style, provider);
            int maxCost = int.Parse(driver.FindElement(By.XPath("//span[@data-role='aw-layered-nav-price-label-to']")).Text.Trim(), style, provider);

            double[] actualValues = Array.ConvertAll(driver.FindElements(By.XPath("//div[@class='price-box price-final_price']//span[@class='price']"))
               .Select(webPrice => webPrice.Text).ToArray<string>(), s => double.Parse(s, style, provider));
            actualValues.ToList().ForEach(actualPrice => Assert.True(actualPrice >= minCost && actualPrice <= maxCost, "Price filter works wrong. Actual price is " + actualPrice + ". But should be more or equal than" + 500 + "and less or equal than" + 300));
        }
        [Test]
        public void TestTooltipText()
        {
            driver.FindElement(By.XPath("//a[@href='https://www.richersounds.com/tv-projectors.html' and @role='menuitem']")).Click();
            driver.FindElement(By.XPath("//img[@title='HDMI Cables']")).Click();
            driver.FindElement(By.XPath("//div[@class='products wrapper grid products-grid']//img[@class='product-image-photo']")).Click();

            new WebDriverWait(driver, TimeSpan.FromSeconds(3))
                .Until(x => driver.FindElements(By.XPath("//button[@title='Add to Cart']")).Any());

            Assert.AreEqual("Add to Cart", driver.FindElement(By.XPath("//button[@title='Add to Cart']")).GetAttribute("title"), "Tooltip has not appeared.");
        }
        [Test]
        public void NegativeSignUpTest()
        {
            // заходим в соответствующий раздел на сайте
            driver.FindElement(By.XPath("//a[@href='https://www.richersounds.com/customer/account/login/']")).Click();
            //переходим к регестрации
            driver.FindElement(By.XPath("//*[contains(text(), 'New to Richer Sounds?')]")).Click();
            driver.FindElement(By.Id("telephone")).SendKeys("+79841465720");
            driver.FindElement(By.Id("age-check")).Click();

            driver.FindElement(By.Id("email_address")).SendKeys("vfbdhjsk57bs442@mail.ru");
            driver.FindElement(By.Id("password")).SendKeys("zK%N12Qb");

            driver.FindElement(By.XPath("//button[@title='Create an Account']")).Click();
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
