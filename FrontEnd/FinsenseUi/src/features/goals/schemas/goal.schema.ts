import { z } from 'zod'

export const GoalSchema = z.object({
  goalName: z
    .string()
    .min(1, 'Goal name is required')
    .max(100, 'Goal name cannot exceed 100 characters'),
  targetAmount: z
    .coerce
    .number()
    .positive('Target amount must be greater than 0')
    .max(1000000000, 'Target amount cannot exceed ₹1,000,000,000'),
  targetDate: z
    .string()
    .min(1, 'Target date is required')
    .refine(val => new Date(val) > new Date(), {
      message: 'Target date must be in the future'
    }),
  currentSavings: z
    .coerce
    .number()
    .min(0, 'Current savings cannot be negative')
    .max(1000000000, 'Current savings cannot exceed ₹1,000,000,000')
}).refine(data => data.currentSavings <= data.targetAmount, {
  message: 'Current savings cannot exceed target amount',
  path: ['currentSavings']
})

export type GoalFormData = z.infer<typeof GoalSchema>
export type GoalUpdateFormData = Partial<GoalFormData>
