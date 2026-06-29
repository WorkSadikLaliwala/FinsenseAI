import { PieChart, Pie, Cell, Tooltip, ResponsiveContainer, Legend } from 'recharts'
import type { CategoryBreakdown } from '../../../types/analytics.types'
import { formatRupee } from '../../../utils/currency'

interface SpendingPieChartProps {
  data: CategoryBreakdown[]
}

const CategoryColors: Record<string, string> = {
  Food: '#F97316',        // orange
  Transport: '#3B82F6',   // blue
  EMI: '#EF4444',         // red
  Entertainment: '#A855F7', // purple
  Utilities: '#10B981',   // green
  Shopping: '#F59E0B',    // amber
  Health: '#06B6D4',      // cyan
  Education: '#6366F1',   // indigo
  Salary: '#22C55E',      // green
  Refund: '#14B8A6',      // teal
  Rent: '#EC4899',        // pink
  'Cash/ATM': '#84CC16',  // lime
  Maintenance: '#0EA5E9',  // light blue
  Others: '#9CA3AF'       // gray
}

export function SpendingPieChart({ data }: SpendingPieChartProps) {
  const filteredData = data.filter(d => d.amount > 0)

  if (filteredData.length === 0) {
    return (
      <div className="h-full flex items-center justify-center text-slate-500 text-xs">
        No spending data available.
      </div>
    )
  }

  return (
    <div className="w-full h-[18rem]">
      <ResponsiveContainer width="100%" height="100%">
        <PieChart>
          <Pie
            data={filteredData}
            dataKey="amount"
            nameKey="category"
            cx="50%"
            cy="50%"
            innerRadius={60}
            outerRadius={80}
            paddingAngle={2}
          >
            {filteredData.map((entry, index) => (
              <Cell
                key={`cell-${index}`}
                fill={CategoryColors[entry.category] ?? CategoryColors.Others}
              />
            ))}
          </Pie>
          <Tooltip
            contentStyle={{
              backgroundColor: '#1e293b',
              borderColor: '#334155',
              borderRadius: '12px',
              fontSize: '11px',
              color: '#fff'
            }}
            formatter={(value: any) => [formatRupee(Number(value)), 'Amount']}
          />
          <Legend
            verticalAlign="bottom"
            iconType="circle"
            iconSize={8}
            wrapperStyle={{ fontSize: '11px', color: '#94a3b8' }}
          />
        </PieChart>
      </ResponsiveContainer>
    </div>
  )
}
