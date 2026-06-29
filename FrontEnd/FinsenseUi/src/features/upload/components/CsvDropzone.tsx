import { useState, useRef } from 'react'
import { UploadCloud, Loader2, AlertTriangle } from 'lucide-react'

interface CsvDropzoneProps {
  onUpload: (file: File) => void
  isPending: boolean
}

export function CsvDropzone({ onUpload, isPending }: CsvDropzoneProps) {
  const [isDragActive, setIsDragActive] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const fileInputRef = useRef<HTMLInputElement>(null)

  const handleDrag = (e: React.DragEvent) => {
    e.preventDefault()
    e.stopPropagation()
    if (e.type === "dragenter" || e.type === "dragover") {
      setIsDragActive(true)
    } else if (e.type === "dragleave") {
      setIsDragActive(false)
    }
  }

  const validateAndUpload = (file: File) => {
    setError(null)

    // Check if CSV
    if (file.type !== "text/csv" && !file.name.endsWith(".csv")) {
      setError("Please upload a valid CSV file (.csv)")
      return
    }

    // Size limit: 5MB
    if (file.size > 5 * 1024 * 1024) {
      setError("File size exceeds 5MB limit")
      return
    }

    onUpload(file)
  }

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault()
    e.stopPropagation()
    setIsDragActive(false)

    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
      validateAndUpload(e.dataTransfer.files[0])
    }
  }

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    e.preventDefault()
    if (e.target.files && e.target.files[0]) {
      validateAndUpload(e.target.files[0])
    }
  }

  const onButtonClick = () => {
    fileInputRef.current?.click()
  }

  return (
    <div className="w-full max-w-xl mx-auto">
      {/* Drop Zone Box */}
      <div
        onDragEnter={handleDrag}
        onDragOver={handleDrag}
        onDragLeave={handleDrag}
        onDrop={handleDrop}
        onClick={onButtonClick}
        className={`w-full min-h-[16rem] border-2 border-dashed rounded-2xl p-8 flex flex-col items-center justify-center cursor-pointer transition-all duration-200 relative overflow-hidden group
          ${isDragActive
            ? 'border-emerald-500 bg-emerald-500/5'
            : 'border-slate-800 bg-slate-900/40 hover:border-slate-700 hover:bg-slate-900/60'
          }
        `}
      >
        <input
          ref={fileInputRef}
          type="file"
          className="hidden"
          accept=".csv"
          onChange={handleChange}
          disabled={isPending}
        />

        {isPending ? (
          <div className="flex flex-col items-center text-center space-y-4">
            <div className="w-12 h-12 bg-emerald-500/10 rounded-xl flex items-center justify-center border border-emerald-500/20 animate-pulse">
              <Loader2 className="w-6 h-6 text-emerald-400 animate-spin" />
            </div>
            <div>
              <h4 className="text-white font-semibold text-sm">Processing Financial Statement...</h4>
              <p className="text-slate-400 text-xs mt-1">Analyzing transactions and categorizing balances</p>
            </div>
          </div>
        ) : (
          <div className="flex flex-col items-center text-center">
            <div className="w-12 h-12 bg-slate-800 rounded-xl flex items-center justify-center mb-4 group-hover:bg-emerald-500/10 border border-slate-700 group-hover:border-emerald-500/20 transition-all duration-200">
              <UploadCloud className="w-6 h-6 text-slate-400 group-hover:text-emerald-400 transition-colors" />
            </div>

            <h4 className="text-white font-semibold text-sm mb-1.5">
              Drag & drop your CSV statement
            </h4>
            <p className="text-slate-500 text-xs mb-3">
              or <span className="text-emerald-400 group-hover:text-emerald-300 font-medium">browse files</span> on your computer
            </p>
            <p className="text-slate-600 text-[10px] uppercase tracking-wider font-semibold">
              Supports standard bank statements (Max 5MB)
            </p>
          </div>
        )}
      </div>

      {/* Error Message */}
      {error && (
        <div className="mt-4 p-3 rounded-xl bg-rose-500/10 border border-rose-500/20 text-rose-400 text-xs flex items-center gap-2">
          <AlertTriangle className="w-4 h-4 flex-shrink-0" />
          <span>{error}</span>
        </div>
      )}
    </div>
  )
}
