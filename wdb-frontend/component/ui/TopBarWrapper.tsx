'use client'

import { usePathname } from 'next/navigation'
import TopBar from '@/component/ui/TopBar'

const HIDDEN_PATHS = ['/', '/register']

export default function TopBarWrapper() {
    const pathname = usePathname()

    if (HIDDEN_PATHS.includes(pathname)) return null

    return <TopBar />
}