import { z } from 'zod'

export const EMISchema = z.object({
  salary: z
    .coerce
    .number()
    .positive('Salary must be greater than 0')
    .max(10000000, 'Salary cannot exceed ₹10,000,000'),
  monthlyExpenses: z
    .coerce
    .number()
    .min(0, 'Expenses cannot be negative')
    .max(10000000, 'Expenses cannot exceed ₹10,000,000'),
  loanAmount: z
    .coerce
    .number()
    .positive('Loan amount must be greater than 0')
    .max(1000000000, 'Loan amount cannot exceed ₹1,000,000,000'),
  tenureMonths: z
    .coerce
    .number()
    .min(1, 'Minimum 1 month')
    .max(360, 'Maximum 360 months'),
  interestRatePerAnnum: z
    .coerce
    .number()
    .min(0.1, 'Minimum interest rate is 0.1%')
    .max(50, 'Maximum interest rate is 50%')
})

export type EMIFormData = z.infer<typeof EMISchema>
