import {
  type HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr'
import { useEffect, useRef, useState } from 'react'
import { csrfHeaders } from '@/api/csrf'
import type { Activity } from './useActivityFeed'

export type LiveStatus = 'connecting' | 'live' | 'offline'

interface LiveHandlers {
  onAdded: (entry: Activity) => void
  onInvalidated: () => void
}

const RETRY_DELAY_MS = 5_000

export function useActivityLive(cityId: string | null, handlers: LiveHandlers) {
  const [status, setStatus] = useState<LiveStatus>('connecting')
  const handlersRef = useRef(handlers)
  const cityRef = useRef(cityId)
  const connectionRef = useRef<HubConnection | null>(null)

  useEffect(() => {
    handlersRef.current = handlers
  })

  useEffect(() => {
    let disposed = false
    let retryTimer: ReturnType<typeof setTimeout> | undefined

    const connection = new HubConnectionBuilder()
      .withUrl('/hubs/activity', { headers: { ...csrfHeaders } })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.None)
      .build()
    connectionRef.current = connection

    connection.on('ActivityAdded', (entry: Activity) => {
      handlersRef.current.onAdded(entry)
    })
    connection.on('ActivityInvalidated', () => {
      handlersRef.current.onInvalidated()
    })
    connection.onreconnecting(() => {
      setStatus('connecting')
    })
    connection.onreconnected(async () => {
      await connection.invoke('Watch', cityRef.current)
      setStatus('live')
      handlersRef.current.onInvalidated()
    })
    connection.onclose(() => {
      if (!disposed) {
        setStatus('offline')
        retryTimer = setTimeout(start, RETRY_DELAY_MS)
      }
    })

    async function start() {
      if (disposed) {
        return
      }
      setStatus('connecting')
      try {
        await connection.start()
        await connection.invoke('Watch', cityRef.current)
        if (!disposed) {
          setStatus('live')
        }
      } catch {
        if (!disposed) {
          setStatus('offline')
          retryTimer = setTimeout(start, RETRY_DELAY_MS)
        }
      }
    }

    void start()

    return () => {
      disposed = true
      clearTimeout(retryTimer)
      connectionRef.current = null
      void connection.stop()
    }
  }, [])

  useEffect(() => {
    cityRef.current = cityId
    const connection = connectionRef.current
    if (connection?.state === HubConnectionState.Connected) {
      connection.invoke('Watch', cityId).catch(() => undefined)
    }
  }, [cityId])

  return status
}
