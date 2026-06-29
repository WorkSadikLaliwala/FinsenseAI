import { useState, useRef, useEffect } from 'react'
import { useSignalR } from '../hooks/useSignalR'
import { chatApi } from '../api/chat.api'
import { useSessionStore } from '../../../store/session.store'
import { GuestBanner } from '../../../components/layout/GuestBanner'
import {
  Send,
  Loader2,
  Trash2,
  Bot,
  User,
  ArrowRight,
  MessageSquare,
  Sparkles,
  Info
} from 'lucide-react'
import { Link } from 'react-router-dom'

function formatAIMessage(text: string) {
  const lines = text.split('\n');

  return lines.map((line, lineIndex) => {
    let content = line.trim();
    const isBullet = content.startsWith('* ');
    if (isBullet) {
      content = content.substring(2);
    }

    const parts: React.ReactNode[] = [];
    const regex = /(\*\*.*?\*\*|\b\d{4}-\d{2}-\d{2}\b|₹\d+(?:\.\d{2})?)/g;
    const tokens = content.split(regex);

    tokens.forEach((token, tokenIndex) => {
      const key = `${lineIndex}-${tokenIndex}`;
      if (token.startsWith('**') && token.endsWith('**')) {
        const cleanBold = token.slice(2, -2);
        parts.push(<strong key={key} className="font-bold text-white">{cleanBold}</strong>);
      } else if (/^\b\d{4}-\d{2}-\d{2}\b$/.test(token)) {
        try {
          const dateObj = new Date(token);
          if (!isNaN(dateObj.getTime())) {
            const formatted = dateObj.toLocaleDateString('en-US', {
              day: '2-digit',
              month: 'short',
              year: 'numeric'
            });
            parts.push(
              <span key={key} className="inline-flex items-center px-1.5 py-0.5 rounded bg-slate-950 border border-slate-800 text-[10px] text-emerald-400 font-semibold select-none mx-0.5">
                {formatted}
              </span>
            );
          } else {
            parts.push(token);
          }
        } catch {
          parts.push(token);
        }
      } else if (/^₹\d+/.test(token)) {
        parts.push(<span key={key} className="text-emerald-400 font-semibold">{token}</span>);
      } else {
        parts.push(token);
      }
    });

    if (isBullet) {
      return (
        <div key={lineIndex} className="flex items-start gap-2 pl-2 py-1">
          <span className="text-emerald-400 select-none mt-1 text-[8px]">•</span>
          <p className="flex-1 text-slate-350">{parts}</p>
        </div>
      );
    }

    return (
      <p key={lineIndex} className={`text-slate-300 min-h-[1rem] ${line.trim() === '' ? 'h-3' : 'py-0.5'}`}>
        {parts}
      </p>
    );
  });
}

export function ChatPage() {
  const { sessionId, isGuest, guestMessageCount } = useSessionStore()
  const [inputText, setInputText] = useState('')
  const [limitReached, setLimitReached] = useState(false)
  const messagesEndRef = useRef<HTMLDivElement>(null)

  const handleLimitReached = () => {
    setLimitReached(true)
  }

  const { messages, isStreaming, sendMessage, clearLocalMessages } = useSignalR({
    sessionId: sessionId ?? '',
    isGuest,
    guestMessageCount,
    onLimitReached: handleLimitReached
  })

  // Scroll to bottom when messages update
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages])

  // Trigger limit on mount if guest count reached
  useEffect(() => {
    if (isGuest && guestMessageCount >= 3) {
      setLimitReached(true)
    }
  }, [isGuest, guestMessageCount])

  const handleSend = (e: React.FormEvent) => {
    e.preventDefault()
    if (!inputText.trim() || isStreaming || limitReached || inputText.length > 500) return
    sendMessage(inputText)
    setInputText('')
  }

  const handleClearHistory = async () => {
    if (!sessionId) return
    if (window.confirm('Are you sure you want to clear your conversation history?')) {
      try {
        await chatApi.deleteChatHistory(sessionId)
        clearLocalMessages()
      } catch (err) {
        console.error('Failed to clear chat history:', err)
      }
    }
  }

  // 1. No active CSV session
  if (!sessionId) {
    return (
      <div className="min-h-[calc(100vh-8rem)] flex items-center justify-center px-4">
        <div className="text-center max-w-md space-y-6">
          <div className="w-16 h-16 bg-slate-900 border border-slate-800 rounded-2xl flex items-center justify-center mx-auto text-emerald-400">
            <Bot className="w-8 h-8" />
          </div>
          <div>
            <h2 className="text-xl font-bold text-white mb-2">AI Advisor Awaiting Statement</h2>
            <p className="text-slate-400 text-sm leading-relaxed">
              Upload your bank statement CSV file first to provide the AI Advisor with your budget context.
            </p>
          </div>
          <Link
            to="/upload"
            className="inline-flex items-center gap-2 px-5 py-2.5 bg-emerald-600 hover:bg-emerald-500 text-white rounded-xl font-semibold text-sm shadow-lg shadow-emerald-600/15 transition-all"
          >
            <span>Upload Statement</span>
            <ArrowRight className="w-4 h-4" />
          </Link>
        </div>
      </div>
    )
  }

  return (
    <div className="py-8 px-6 max-w-4xl mx-auto h-[calc(100vh-4rem)] flex flex-col space-y-4">
      {/* Chat Title & Actions Header */}
      <div className="flex items-center justify-between border-b border-slate-800 pb-4 flex-shrink-0">
        <div className="flex items-center gap-3">
          <div className="w-9 h-9 bg-emerald-500/10 rounded-xl flex items-center justify-center border border-emerald-500/20 text-emerald-400">
            <Bot className="w-5 h-5" />
          </div>
          <div>
            <h1 className="text-base font-bold text-white flex items-center gap-2">
              <span>FinSense AI Advisor</span>
              <span className="px-1.5 py-0.5 rounded-full bg-emerald-500/10 text-emerald-400 border border-emerald-500/20 text-[9px] font-semibold flex items-center gap-1 select-none animate-pulse">
                <Sparkles className="w-2.5 h-2.5 text-emerald-400" />
                <span>Llama 3 (Groq API)</span>
              </span>
            </h1>
            <p className="text-[10px] text-slate-400">Contextualized personal budget assistant</p>
          </div>
        </div>

        {messages.length > 0 && (
          <button
            onClick={handleClearHistory}
            className="flex items-center gap-1.5 px-3 py-1.5 bg-slate-900 border border-slate-800 hover:border-rose-500/30 hover:bg-rose-500/5 text-slate-400 hover:text-rose-400 rounded-lg text-xs font-semibold transition-all duration-200"
          >
            <Trash2 className="w-3.5 h-3.5" />
            <span>Clear History</span>
          </button>
        )}
      </div>

      {/* Chat Messages Log Area */}
      <div className="flex-1 overflow-y-auto pr-2 space-y-4 min-h-0">
        {messages.length === 0 ? (
          <div className="h-full flex flex-col items-center justify-center text-center max-w-md mx-auto space-y-5">
            <div className="w-10 h-10 bg-slate-900 border border-slate-850 rounded-xl flex items-center justify-center text-slate-500">
              <MessageSquare className="w-5 h-5" />
            </div>
            <div>
              <h4 className="text-white text-xs font-bold">Ask AI Anything</h4>
              <p className="text-slate-500 text-[11px] leading-relaxed mt-1">
                Ask questions about your budget, transactions, and credit allocations.
              </p>
            </div>
            <div className="pt-2 flex flex-col items-center gap-2 w-full select-none">
              <span className="text-[10px] font-semibold text-slate-500 uppercase tracking-wider">Tap a suggestion to ask:</span>
              <div className="flex flex-col gap-2 w-full max-w-xs">
                {[
                  "Where did I spend the most money?",
                  "How can I reduce my Food category spending?",
                  "Analyze if my monthly EMI budget is safe"
                ].map((preset) => (
                  <button
                    key={preset}
                    type="button"
                    onClick={() => setInputText(preset)}
                    className="w-full px-3 py-2 bg-slate-950 border border-slate-850 hover:border-emerald-500/30 text-slate-400 hover:text-white rounded-xl text-[11px] font-medium text-left transition-all hover:bg-slate-900"
                  >
                    {preset}
                  </button>
                ))}
              </div>
            </div>
          </div>
        ) : (
          messages.map((msg) => {
            const isUser = msg.role === 'user'
            return (
              <div
                key={msg.id}
                className={`flex gap-3 max-w-[85%]
                  ${isUser ? 'ml-auto flex-row-reverse' : ''}
                `}
              >
                {/* Avatar */}
                <div className={`w-8 h-8 rounded-full flex items-center justify-center flex-shrink-0 border
                  ${isUser
                    ? 'bg-slate-800 border-slate-700 text-slate-300'
                    : 'bg-emerald-500/10 border-emerald-500/20 text-emerald-400'
                  }
                `}>
                  {isUser ? <User className="w-4 h-4" /> : <Bot className="w-4 h-4" />}
                </div>

                {/* Bubble */}
                <div className="space-y-1">
                  <div className={`px-4 py-3 rounded-2xl text-xs leading-relaxed whitespace-pre-wrap shadow-md
                    ${isUser
                      ? 'bg-emerald-600 text-white rounded-tr-none'
                      : 'bg-slate-900 border border-slate-800 text-slate-200 rounded-tl-none'
                    }
                  `}>
                    {msg.content === '' && isStreaming ? (
                      <div className="flex items-center gap-1.5 text-slate-400 py-0.5">
                        <Loader2 className="w-3.5 h-3.5 animate-spin" />
                        <span>AI is thinking...</span>
                      </div>
                    ) : isUser ? (
                      msg.content
                    ) : (
                      formatAIMessage(msg.content)
                    )}
                  </div>
                  <span className="text-[9px] text-slate-500 block px-1">
                    {msg.timestamp.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                  </span>
                </div>
              </div>
            )
          })
        )}
        <div ref={messagesEndRef} />
      </div>

      {/* Input / Limit Banner Container */}
      <div className="flex-shrink-0 pt-2 border-t border-slate-800">
        {limitReached ? (
          <div className="space-y-2">
            <div className="p-3 bg-amber-500/5 border border-amber-500/10 rounded-xl flex items-center gap-2 text-xs text-amber-400">
              <Info className="w-4.5 h-4.5 flex-shrink-0" />
              <span>You have reached your limit of 3 free questions. Please sign up to unlock unlimited conversation.</span>
            </div>
            <GuestBanner />
          </div>
        ) : (
          <form onSubmit={handleSend} className="space-y-1.5">
            <div className="flex gap-2">
              <input
                type="text"
                value={inputText}
                onChange={(e) => setInputText(e.target.value)}
                disabled={isStreaming}
                placeholder={
                  isGuest
                    ? `Ask AI a question (${3 - guestMessageCount} free left)...`
                    : "Ask AI advisor about your finances..."
                }
                className={`flex-1 px-4 py-3 bg-slate-900 border focus:ring-1 focus:ring-emerald-500/10 rounded-xl text-xs text-white placeholder-slate-650 outline-none transition-all disabled:opacity-60
                  ${inputText.length > 500 ? 'border-rose-500 focus:border-rose-500/30' : 'border-slate-800 focus:border-emerald-500/30'}
                `}
              />
              <button
                type="submit"
                disabled={!inputText.trim() || isStreaming || inputText.length > 500}
                className="px-4 bg-emerald-600 hover:bg-emerald-500 text-white rounded-xl flex items-center justify-center transition-all duration-200 disabled:opacity-50"
              >
                {isStreaming ? (
                  <Loader2 className="w-4 h-4 animate-spin" />
                ) : (
                  <Send className="w-4 h-4" />
                )}
              </button>
            </div>
            <div className="flex justify-between items-center text-[10px] px-1 select-none">
              {inputText.length > 500 ? (
                <span className="text-rose-400 font-medium">Message cannot exceed 500 characters</span>
              ) : (
                <span className="text-slate-500">Press Enter to send</span>
              )}
              <span className={`font-semibold ${inputText.length > 500 ? 'text-rose-400' : 'text-slate-500'}`}>
                {inputText.length}/500
              </span>
            </div>
          </form>
        )}
      </div>
    </div>
  )
}
