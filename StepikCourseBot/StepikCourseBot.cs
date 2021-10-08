using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace StepikCourseBot {
    internal static class StepikCourseBot {
        #region WebDriver and Url info

        private static ChromeDriver _driver;
        private static WebDriverWait _waitDriver;

        private const string BaseUrl = "https://stepik.org/lesson/106620/";
        private static string _stepUrl = "step/1?";
        private static string _loginUrl = "auth=login&";
        private static string _unitUrl = "unit=81144";
        private const string CourseEndedString = "lesson-end";

        #endregion

        #region User Credentials

        private static string _userEmail;
        private static string _userPassword;

        #endregion

        #region Web Locators

        private static readonly By LoginButtonLocator = By.ClassName("sign-form__btn");
        private static readonly By LoginEmailLocator = By.Name("login");
        private static readonly By LoginPasswordLocator = By.Name("password");


        private static readonly By CourseProgressLocator = By.ClassName("lesson-sidebar__course-progress");

        private static readonly By AttemptInnerLocator = By.ClassName("attempt__inner");
        private static readonly By TestTypeLocator = By.ClassName("quiz-component");
        private static readonly By TestTypeRadioLocator = By.ClassName("s-radio");
        private static readonly By AttemptsLimitLocator = By.ClassName("cost-info__desc-toggler");
        private static readonly By AttemptMessageCorrectLocator = By.ClassName("attempt-message_correct");
        private static readonly By AttemptMessageWrongLocator = By.ClassName("attempt-message_wrong");
        private static readonly By SubmitTestLocator = By.ClassName("submit-submission");
        private static readonly By RetryTestLocator = By.ClassName("again-btn");
        private static readonly By NextStepLocator = By.ClassName("lesson__next-btn");

        #endregion

        private static string Url(bool login) =>
            login ? BaseUrl + _stepUrl + _loginUrl + _unitUrl : BaseUrl + _stepUrl + _unitUrl;

        private static void Main() {
            Console.WriteLine("Starting bot");
            
            EnterCredentials();
            SetUpWebDriver();
            StartBot();
            LogIn();

            var progresses = GetCourseProgress();
            while (progresses[0] != progresses[1]) {
                CompleteStep();
                GoToNextStep();
            }

            TearDownWebDriver();
        }

        private static void SetUpWebDriver() {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            _driver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory);
            _waitDriver = new WebDriverWait(_driver, TimeSpan.FromSeconds(3));
        }

        private static void TearDownWebDriver() {
            _driver.Quit();
        }

        private static void StartBot() {
            _driver.Navigate().GoToUrl(Url(true));
        }

        /*  TODO
         * 0. [READY] Login to account
         * 1. [READY] Check if can rerun test
         * 2. [READY] Complete radioButton test
         * 3. [READY] Go to next step / next unit
         * 4. ???
         */

        private static void WaitAndClick(By element) {
            _waitDriver.Until(driver => driver.FindElement(element));
            _driver.FindElement(element).Click();
        }

        private static void EnterCredentials() {
            Console.WriteLine("Enter stepik email:");
            _userEmail = Console.ReadLine();
            Console.WriteLine("Enter stepik password:");
            _userPassword = Console.ReadLine();
        }
        
        private static void LogIn() {
            _waitDriver.Until(driver => driver.FindElement(LoginEmailLocator));

            var loginEmail = _driver.FindElement(LoginEmailLocator);
            var loginPassword = _driver.FindElement(LoginPasswordLocator);
            var loginButton = _driver.FindElement(LoginButtonLocator);

            loginEmail.SendKeys(_userEmail);
            loginPassword.SendKeys(_userPassword);
            loginButton.Click();

            WriteCourseProgress(GetCourseProgress());
        }

        private static string[] GetCourseProgress() {
            try {
                _waitDriver.Until(driver => driver.FindElement(CourseProgressLocator));

                var courseProgress = _driver.FindElement(CourseProgressLocator);
                var courseProgressText = Regex.Replace(courseProgress.Text, @"[A-Za-zА-Яа-я\s\b:]+", "");
                var courseProgresses = courseProgressText.Split('/');
                var courseProgressCurrent = courseProgresses[0];
                var courseProgressMaximum = courseProgresses[1];

                return new[] { courseProgressCurrent, courseProgressMaximum };
            }
            catch (Exception e) {
                // Console.WriteLine(e);
                _driver.Navigate().Refresh();
                System.Threading.Thread.Sleep(5000);
                return GetCourseProgress();
            }
        }

        private static void WriteCourseProgress(string[] progress) {
            Console.WriteLine($"Current course progress: {progress[0]}");
        }

        private static bool CheckIfStepHasTest() {
            try {
                _driver.FindElement(AttemptInnerLocator);
            }
            catch (Exception e) {
                // Console.WriteLine(e);
                return false;
            }

            return true;
        }

        private static bool CheckIfCanRerunTest() {
            try {
                System.Threading.Thread.Sleep(500);
                _driver.FindElement(AttemptsLimitLocator);
                // WaitAndClick(AttemptsLimitLocator);
            }
            catch (Exception e) {
                // Console.WriteLine(e);
                return true;
            }

            return false;
        }

        //  TODO Rename
        private static ReadOnlyCollection<IWebElement> CheckIfTestHasRadios() {
            try {
                _waitDriver.Until(driver => driver.FindElement(TestTypeLocator));
                return _driver.FindElements(TestTypeRadioLocator);
            }
            catch (Exception e) {
                // Console.WriteLine(e);
                return null;
            }
        }

        private static void CompleteStep() {
            if (!CheckIfStepHasTest()) return;
            if (!CheckIfCanRerunTest()) return;
            if (WaitForAttemptMessage()) return;

            var radioButtons = CheckIfTestHasRadios();
            if (radioButtons?.Count > 0) {
                TrySolveStep();
            }
        }

        //  TODO Update method to save recently used radios texts
        private static void TrySolveStep() {
            do {
                try {
                    _waitDriver.Until(driver => driver.FindElement(TestTypeLocator));
                    var radioButtons =  _driver.FindElements(TestTypeRadioLocator);
                    radioButtons[0].Click();

                    WaitAndClick(SubmitTestLocator);
                    if (WaitForAttemptMessage()) {
                        WriteCourseProgress(GetCourseProgress());
                        return;
                    }

                    WaitAndClick(RetryTestLocator);
                    System.Threading.Thread.Sleep(500);
                }
                catch (Exception e) {
                    // Console.WriteLine(e);
                    _driver.Navigate().Refresh();
                    System.Threading.Thread.Sleep(5000);
                }
            } while (true);
        }

        private static bool WaitForAttemptMessage() {
            try {
                System.Threading.Thread.Sleep(1000);
                _driver.FindElement(AttemptMessageCorrectLocator);
                return true;
            }
            catch (Exception e) {
                // Console.WriteLine(e);
            }

            try {
                System.Threading.Thread.Sleep(1000);
                _driver.FindElement(AttemptMessageWrongLocator);
                return false;
            }
            catch (Exception e) {
                // Console.WriteLine(e);
            }

            return false;
        }

        private static void GoToNextStep() {
            WaitAndClick(NextStepLocator);

            if (CheckIfCourseEnded()) {
                GetCourseProgress();
                Console.WriteLine("Goodbye!");
            }
        }

        private static bool CheckIfCourseEnded() {
            return _driver.Url.Contains(CourseEndedString);
        }
    }
}