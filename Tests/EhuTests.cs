using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Linq;

namespace WebUiAutomation.Tests
{
    [TestFixture]
    public class EhuTests
    {
        private IWebDriver driver = default!;
        private const string BaseEn = "https://en.ehuniversity.lt/";
        private const string BaseLt = "https://lt.ehuniversity.lt/";

        [SetUp]
        public void SetUp()
        {
            // Start Chrome
            var options = new ChromeOptions();
            driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        [TearDown]
        public void TearDown()
        {
            try { driver.Quit(); } catch { /* ignore */ }
        }

        // Helper: Click using JavaScript (to avoid issues with elements not clickable)
        private void JsClick(IWebElement el)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", el);
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", el);
        }

        // Test Case 1: About page
        [Test]
        public void AboutPage_Should_Open()
        {
            // Open home page (use BaseEn constant)
            driver.Navigate().GoToUrl(BaseEn);

            // Click "About" in the main menu
            var about =
                driver.FindElements(By.LinkText("About")).FirstOrDefault() ??
                driver.FindElements(By.CssSelector("a[href*='/about/']")).FirstOrDefault();

            Assert.That(about, Is.Not.Null, "About menu link was not found.");
            JsClick(about!);

            // Expect: URL contains /about and title contains "About"
            Assert.That(driver.Url.ToLower(), Does.Contain("/about"), "URL does not contain '/about'.");
            Assert.That(driver.Title.ToLower(), Does.Contain("about"), "Title does not contain 'About'.");

            // Expect: H1 contains "About"
            var h1 = driver.FindElements(By.CssSelector("h1")).FirstOrDefault();
            Assert.That(h1, Is.Not.Null, "H1 was not found on the About page.");
            Assert.That(h1!.Text.Trim().ToLower(), Does.Contain("about"), "H1 does not contain 'About'.");
        }

        // Test Case 2: Search "study programs"
        [Test]
        public void Search_Should_Return_Results_For_Study_Programs()
        {
            driver.Navigate().GoToUrl(BaseEn);

            // Try to find a real search input; if it is not interactable, fall back to direct query URL
            var search =
                driver.FindElements(By.CssSelector("input[type='search']")).FirstOrDefault() ??
                driver.FindElements(By.Name("s")).FirstOrDefault();

            if (search != null && search.Enabled && search.Displayed)
            {
                search.Clear();
                search.SendKeys("study programs" + Keys.Enter);
            }
            else
            {
                // Fallback: open search results directly
                driver.Navigate().GoToUrl(BaseEn + "?s=study+programs");
            }

            // Expect: URL contains '?s=study+programs' and page contains word 'study'
            Assert.That(driver.Url.ToLower(), Does.Contain("?s=study+programs"),
                "URL does not contain '?s=study+programs'.");
            Assert.That(driver.PageSource.ToLower(), Does.Contain("study"),
                "Search results page does not contain the word 'study'.");
        }

        // Test Case 3: Language switch to LT
        [Test]
        public void Language_Should_Switch_To_Lithuanian()
        {
            driver.Navigate().GoToUrl(BaseEn);

            // Current site uses subdomains: lt.ehuniversity.lt
            var lt =
                driver.FindElements(By.CssSelector("a[href*='lt.ehuniversity.lt']")).FirstOrDefault() ??
                driver.FindElements(By.PartialLinkText("lt")).FirstOrDefault() ??
                driver.FindElements(By.PartialLinkText("Lietuvi")).FirstOrDefault();

            Assert.That(lt, Is.Not.Null, "Lithuanian language link was not found.");
            JsClick(lt!);

            // Expect: moved to Lithuanian site and Lithuanian content present
            Assert.That(driver.Url, Does.StartWith(BaseLt), "URL is not Lithuanian version.");
            Assert.That(driver.PageSource.ToLower(), Does.Contain("apie"),
                "Lithuanian content was not detected (expected 'Apie').");
        }

        // Test Case 4: Contacts page (info visible)
        [Test]
        public void Contact_Info_Should_Be_Visible()
        {
            driver.Navigate().GoToUrl(BaseEn + "contacts/");

            var pageLower = driver.PageSource.ToLower();

            // Current contacts on the site
            Assert.That(pageLower, Does.Contain("contacts"), "Contacts header is missing.");
            Assert.That(pageLower, Does.Contain("consult@ehu.lt"), "consult@ehu.lt is missing.");
            Assert.That(pageLower, Does.Contain("press@ehu.lt"), "press@ehu.lt is missing.");
            Assert.That(pageLower, Does.Contain("office@ehu.lt"), "office@ehu.lt is missing.");
            Assert.That(pageLower, Does.Contain("+370 5 263 9650"), "Phone +370 5 263 9650 is missing.");

            // one more phone from the page
            Assert.That(pageLower, Does.Contain("(644) 96 317"), "Mobile phone is missing.");

            // Socials present on the page:
            Assert.That(pageLower, Does.Contain("facebook.com"), "Facebook link is missing.");
            Assert.That(pageLower, Does.Contain("youtube.com"), "YouTube link is missing.");
            Assert.That(pageLower, Does.Contain("instagram.com"), "Instagram link is missing.");
            Assert.That(pageLower, Does.Contain("linkedin.com"), "LinkedIn link is missing.");
        }
    }
}
