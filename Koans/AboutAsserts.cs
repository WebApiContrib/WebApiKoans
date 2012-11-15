using FSharpKoans.Core;

namespace Koans
{
    /**
     * Getting Started
     *
     * The Web API Koans are a set of exercises designed to get you familiar 
     * with ASP.NET Web API. By the time you're done, you'll have a basic 
     * understanding of the components and extensibility points in Web API.
     *
     * Answering Problems
     * 
     * This is where the fun begins! Each Koan method contains
     * an example designed to teach you a lesson about the F# language. 
     * If you exectue the program defined in this project, you will get
     * a message that the AssertEquality koan below has failed. Your
     * job is to fill in the blank (the __ symbol) to make it pass. Once
     * you make the change, re-run the program to make sure the koan
     * passes, and continue on to the next failing koan.  With each 
     * passing koan, you'll learn more about Web API, and add another
     * weapon to your Web API programming arsenal.
     */

    [Koan(Sort = 0)]
    public static class AboutAsserts
    {
        [Koan]
        public static void AssertExpectation()
        {
            var expectedValue = 1 + 1;
            var actualValue = Helpers.__; // start by changing this line

            Helpers.AssertEquality(expectedValue, actualValue);
        }

        [Koan]
        public static void FillInValues()
        {
            Helpers.AssertEquality(1 + 1, Helpers.__);
        }
    }
}
