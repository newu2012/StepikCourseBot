using System;
using System.Text.RegularExpressions;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace StepikCourseBot {
    static class StepikCourseBot {
        #region WebDriver and Url info

        private static ChromeDriver _driver;
        private static WebDriverWait _waitDriver;

        private const string BaseUrl = "https://stepik.org/lesson/106620/";
        private static string _stepUrl = "step/1?";
        private static string _loginUrl = "auth=login&";
        private static string _unitUrl = "unit=81144";

        #endregion

        #region User Credentials

        private const string UserEmail = "";
        private const string UserPassword = "";

        #endregion

        #region Web Locators

        private static readonly By LoginButtonLocator = By.ClassName("sign-form__btn");
        private static readonly By LoginEmailLocator = By.Name("login");
        private static readonly By LoginPasswordLocator = By.Name("password");


        private static readonly By CourseProgressLocator = By.ClassName("lesson-sidebar__course-progress");

        private static readonly By AttemptInnerLocator = By.ClassName("attempt__inner");
        private static readonly By AttemptMessageCorrectLocator = By.ClassName("attempt-message_correct");
        private static readonly By AttemptMessageWrongLocator = By.ClassName("attempt-message_wrong");
        private static readonly By NextStepLocator = By.ClassName("lesson__next-btn");

        #endregion

        private static string Url(bool login) =>
            login ? BaseUrl + _stepUrl + _loginUrl + _unitUrl : BaseUrl + _stepUrl + _unitUrl;

        private static void Main(string[] args) {
            Console.WriteLine("Starting bot");

            SetUpWebDriver();
            StartBot();
            LogIn();


            //  TODO

            TearDownWebDriver();
        }

        private static void SetUpWebDriver() {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            _driver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory);
            _waitDriver = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        }

        private static void TearDownWebDriver() {
            _driver.Quit();
        }

        private static void StartBot() {
            _driver.Navigate().GoToUrl(Url(true));
        }

        /*  TODO
         * 0. Login to account
         * 1. Check if can rerun test
         * 2. Complete radioButton test
         * 3. Go to next step / next unit
         * 4.
         *
         *
         * 
         */

        static void LogIn() {
            _waitDriver.Until(driver => driver.FindElement(LoginEmailLocator));

            var loginEmail = _driver.FindElement(LoginEmailLocator);
            var loginPassword = _driver.FindElement(LoginPasswordLocator);
            var loginButton = _driver.FindElement(LoginButtonLocator);

            loginEmail.SendKeys(UserEmail);
            loginPassword.SendKeys(UserPassword);
            loginButton.Click();

            CheckCourseProgress();
        }

        static void CheckCourseProgress() {
            _waitDriver.Until(driver => driver.FindElement(CourseProgressLocator));

            var courseProgress = _driver.FindElement(CourseProgressLocator);
            var courseProgressText = Regex.Replace(courseProgress.Text, @"[A-Za-zА-Яа-я\s\b:]+", "");
            var courseProgresses = courseProgressText.Split('/');
            var courseProgressCurrent = courseProgresses[0];
            var courseProgressMaximum = courseProgresses[1];

            Console.WriteLine($"Initial course progress: {courseProgressCurrent}");
            Console.WriteLine($"Going to: {courseProgressMaximum}!");
        }

        static void CheckIfCanRerunTest() {
        }
    }
}