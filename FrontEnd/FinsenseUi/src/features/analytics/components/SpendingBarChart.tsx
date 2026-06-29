import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Cell } from 'recharts'
import type { CategoryBreakdown } from '../../../types/analytics.types'
import { formatRupeeCompact } from '../../../utils/currency'

interface SpendingBarChartProps {
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

export function SpendingBarChart({ data }: SpendingBarChartProps) {
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
        <BarChart data={filteredData} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="#1e293b" vertical={false} />
          <XAxis
            dataKey="category"
            stroke="#64748b"
            fontSize={10}
            tickLine={false}
            axisLine={false}
          />
          <YAxis
            stroke="#64748b"
            fontSize={10}
            tickLine={false}
            axisLine={false}
            tickFormatter={(value) => formatRupeeCompact(value)}
          />
          <Tooltip
            contentStyle={{
              backgroundColor: '#1e293b',
              borderColor: '#334155',
              borderRadius: '12px',
              fontSize: '11px',
              color: '#fff'
            }}
            formatter={(value: any) => [formatRupeeCompact(Number(value)), 'Spending']}
            cursor={{ fill: '#1e293b', opacity: 0.4 }}
          />
          <Bar dataKey="amount" radius={[4, 4, 0, 0]} maxBarSize={32}>
            {filteredData.map((entry, index) => (
              <Cell
                key={`cell-${index}`}
                fill={CategoryColors[entry.category] ?? CategoryColors.Others}
              />
            ))}
          </Bar>
        </BarChart>
      </ResponsiveContainer>
    </div>
  )
}
