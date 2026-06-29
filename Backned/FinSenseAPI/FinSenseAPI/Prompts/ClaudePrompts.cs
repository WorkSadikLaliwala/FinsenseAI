//using System;

//namespace FinSenseAPI.Prompts;

//public static class ClaudePrompts
//{
//    public static string TransactionCategorizer(string transactionsJson)
//    {
//        return $@"Categories: [Food, Transport, EMI, Entertainment, Utilities, Shopping, Health, Education, Salary, Refund, Others]
//Input: {transactionsJson}
//Instruction: Assign each transaction a category from the list above and return a JSON array of objects with properties 'description' and 'category'.
//Output: JSON only, no markdown, no explanation.";
//    }

//    public static string EMIAffordabilityEvaluator(decimal salary, decimal expenses, decimal emi, decimal disposable, double percent)
//    {
//        return $@"Input: Salary={salary}, Expenses={expenses}, EMI={emi}, Disposable={disposable}, Percent={percent}
//Instruction: Evaluate affordability and return a JSON object: {{" + "status, reason, recommendation}}.\nStatus must be one of: Safe | Risky | Danger.\nReturn JSON only.";
//    }

//    public static string MonthEndPredictorCommentary(int daysElapsed, decimal spent, decimal dailyAverage, int daysRemaining, decimal balance, decimal income, string status, string topCategory, decimal topAmount)
//    {
//        return $@"Input: daysElapsed={daysElapsed}, spent={spent}, dailyAverage={dailyAverage}, daysRemaining={daysRemaining}, balance={balance}, income={income}, status={status}, topCategory={topCategory}, topAmount={topAmount}
//Instruction: Produce 2-3 sentences of plain text with one actionable tip. Do not use JSON or markdown.";
//    }

//    public static string GoalFeasibilityAnalyzer(string goalName, decimal target, decimal current, int months, decimal required, decimal surplus, string categoriesJson)
//    {
//        return $@"Input: goalName={goalName}, target={target}, current={current}, months={months}, required={required}, surplus={surplus}, categories={categoriesJson}
//Instruction: Analyze feasibility and return a JSON object: {{status, reason, tip}}.\nStatus must be one of: On Track | At Risk | Not Feasible.\nReturn JSON only.";
//    }

//    public static string AIChatAdvisor(string systemContextJson, string userMessage)
//    {
//        return $@"SystemContext: {systemContextJson}
//UserMessage: {userMessage}
//Rules: Answer about personal finance only. Use actual rupee amounts with the ₹ symbol. Keep reply under 4 sentences and include one actionable next step. Use Indian context (UPI, EMI, SIP). Include relevant figures from the context. Do not provide investment/legal advice. Return plain text only.";
//    }
//}

using System;

namespace FinSenseAPI.Prompts;

public static class ClaudePrompts
{

    //    public static string TransactionCategorizer(string transactionsJson)
    //    {
    //        return $@"Categorize each transaction. Return ONLY a JSON array. No markdown, no explanation.

    //Categories: Food, Transport, EMI, Entertainment, Utilities, Shopping, Health, Insurance, Salary, Others

    //Rules:
    //- Swiggy/Zomato/Restaurant/Cafe/Dominos = Food
    //- Uber/Ola/Petrol/Fuel = Transport
    //- Home Loan/Personal Loan/EMI = EMI
    //- Netflix/Spotify/Hotstar/BookMyShow/Prime = Entertainment
    //- Electricity/Water/Mobile Bill/Internet = Utilities
    //- Amazon/DMart/Reliance/Shopping = Shopping
    //- Pharmacy/Medical/Hospital/Gym/Lab = Health
    //- LIC/Insurance/Premium = Insurance
    //- Salary/Freelance/Income = Salary
    //- ATM/UPI/Unknown = Others

    //Input:
    //{transactionsJson}

    //Output format:
    //[{{""description"":""EXACT_DESCRIPTION"",""category"":""Category""}}]";
    //    }

    public static string MonthEndPredictorCommentary(
        int daysElapsed,
        decimal spent,
        decimal dailyAverage,
        int daysRemaining,
        decimal balance,
        decimal income,
        string status,
        string topCategory,
        decimal topAmount)
    {
        return $@"
    Input:
    DaysElapsed = {daysElapsed}
    Spent = {spent}
    DailyAverage = {dailyAverage}
    DaysRemaining = {daysRemaining}
    Balance = {balance}
    Income = {income}
    Status = {status}
    TopCategory = {topCategory}
    TopAmount = {topAmount}

    Instruction:
    Write 2-3 sentences with:
    - spending insight
    - one actionable tip

    Rules:
    - NO JSON
    - NO markdown
    - Plain text only
    ";
    }

    public static string TransactionCategorizer(string descriptionsJson)
    {
        return $@"You are a bank transaction categorizer for Indian users.
Return ONLY a JSON array. No markdown, no explanation, no extra text.

RULES — match in this exact order, first match wins:

EMI: Home Loan, Personal Loan, Car Loan, Vehicle Loan, Education Loan, Gold Loan, EMI, Credit Card Payment, CC Payment, HDFC Credit Card, ICICI Credit Card, SBI Credit Card, Axis Credit Card, Kotak Credit Card, Bajaj Finserv, LazyPay, Simpl
Entertainment: Netflix, Amazon Prime, Prime Video, Prime Subscription, Hotstar, Disney, SonyLiv, Zee5, Voot, JioCinema, AltBalaji, Spotify, Gaana, JioSaavn, Wynk, YouTube Premium, BookMyShow, Book My Show, PVR, INOX, Steam, PlayStation
Food: Swiggy, Zomato, Dunzo, Blinkit, Zepto, Restaurant, Cafe, Coffee, Dominos, Pizza Hut, McDonald, Burger King, KFC, Subway, Starbucks, Barista, Chaayos, Haldirams, Hotel Food, Dhaba, Biryani
Transport: Uber, Ola, Rapido, InDrive, Petrol, Fuel, Diesel, HP Pump, Indian Oil, Bharat Petroleum, Shell, Fastag, Toll, Parking, Metro, IRCTC, RedBus, KSRTC, MSRTC
Utilities: Electricity, Torrent Power, BESCOM, MSEDCL, Tata Power, Adani Electricity, Water Bill, AUDA, BWSSB, Airtel, Jio, Vodafone, Vi, BSNL, Broadband, Internet Bill, Mobile Bill, Gas Bill, PNG, Indane, HP Gas, Bharat Gas, Tata Sky, Dish TV, DTH
Insurance: LIC, Insurance, Premium, HDFC Life, ICICI Prudential, SBI Life, Max Life, Bajaj Allianz, Tata AIA, Star Health, Niva Bupa, Term Plan, ULIP, Vehicle Insurance
Health: Pharmacy, Medical, Apollo Pharmacy, MedPlus, Netmeds, 1mg, PharmEasy, Hospital, Clinic, Diagnostic, Lab, Thyrocare, Dr Lal, Metropolis, Gym, Cult Fit, CureFit, Fitness, Yoga, Practo, Doctor
Salary: Salary, Salary Credit, Freelance, Freelance Payment, Income, Wages, Stipend, Bonus Credit, Commission, Consulting Fees, Project Payment
Shopping: Amazon Purchase, Amazon Order, Flipkart, Myntra, Meesho, Nykaa, Ajio, Snapdeal, DMart, Reliance Fresh, Reliance Smart, Big Bazaar, More Supermarket, Decathlon, Lifestyle, Shoppers Stop, Pantaloons, Westside, Croma, Vijay Sales, Reliance Digital
Rent: Rent, House Rent, Rent Transfer, LANDLORD, Rental, PG Rent, Lease Payment, Rent Payment
Cash/ATM: ATM, Cash Withdrawal, Cash, ATM Cash, Self Transfer, Cash Dispensed
Maintenance: Maintenance, Society Maintenance, Maintenance Charge, Society Bill, Repair, Building Maintenance
Others: UPI Transfer, NEFT Transfer, IMPS, Recharge, Paytm Recharge, Interest Credit, Dividend, Cashback, Miscellaneous

If nothing matches, use Others.

Input (description strings only):
{descriptionsJson}

Output format:
[{{""description"":""EXACT_DESCRIPTION_FROM_INPUT"",""category"":""Category""}}]";
    }

    // ============================
    // ✅ FINAL FIXED GOAL PROMPT
    // ============================
    public static string GoalFeasibilityAnalyzer(
        string goalName,
        decimal target,
        decimal current,
        int months,
        decimal required,
        decimal surplus)
    {
        return $@"
You are a financial planning AI expert.

Analyze the user's savings goal and return STRICT JSON only.

INPUT:
Goal Name: {goalName}
Target Amount: {target}
Current Savings: {current}
Months Remaining: {months}
Required Monthly Saving: {required}
Current Monthly Surplus: {surplus}

TASK:
Evaluate if the user can achieve the goal.

OUTPUT (must include ALL fields):

{{
  ""status"": ""On Track | At Risk | Not Feasible"",
  ""amountRemaining"": number,
  ""monthsRemaining"": number,
  ""requiredMonthlySaving"": number,
  ""currentMonthlySurplus"": number,
  ""shortfall"": number,
  ""reason"": ""string"",
  ""tip"": ""string""
}}

RULES:
- Return ONLY valid JSON
- No markdown
- No explanations
- No extra fields
- Never omit any field
- All numeric values must be numbers
- Keep reasoning short (1-2 lines)

CALCULATION RULES:
- amountRemaining = target - current
- shortfall = max(0, requiredMonthlySaving - currentMonthlySurplus)

Return JSON only.
";
    }

    public static string AIChatAdvisor(string systemContextJson, string userMessage)
    {
        return $@"
SystemContext:
{systemContextJson}

UserMessage:
{userMessage}

Rules:
- Answer only personal finance questions
- Use ₹ currency format (Indian context)
- Max 4 sentences
- Include one actionable suggestion
- Mention relevant numbers from context
- No investment/legal advice
- Plain text only
";
    }
}