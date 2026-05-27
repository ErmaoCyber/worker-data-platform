'use client'

import { WorkerInfoItem } from '../type'

interface UserInfoCardProps {
    data: WorkerInfoItem[]
    workerId: string
    userName: string
}


export default function UserInfoCard({ data, workerId, userName }: UserInfoCardProps) {
    const displayName = data.find(item => item.desc === 'fullName')?.value ?? userName

    return (
        <div className="flex items-center justify-between">
            <h1 className="text-2xl font-semibold text-slate-900">My Profile</h1>
        </div>
    )
}