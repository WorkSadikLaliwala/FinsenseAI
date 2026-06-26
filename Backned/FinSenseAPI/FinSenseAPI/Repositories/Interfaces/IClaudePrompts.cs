//using System;

namespace FinSenseAPI.Repositories.Interfaces
{
    public interface IClaudePrompts
    {
        static abstract string AIChatAdvisor(string systemContextJson, string userMessage);
        static abstract string EMIAffordabilityEvaluator(decimal salary, decimal expenses, decimal emi, decimal disposable, double percent);
        static abstract string GoalFeasibilityAnalyzer(string goalName, decimal target, decimal current, int months, decimal required, decimal surplus);
        static abstract string MonthEndPredictorCommentary(int daysElapsed, decimal spent, decimal dailyAverage, int daysRemaining, decimal balance, decimal income, string status, string topCategory, decimal topAmount);
        static abstract string TransactionCategorizer(string transactionsJson);
    }
}