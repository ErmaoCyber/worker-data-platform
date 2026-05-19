'use client'

import { useState, useEffect } from 'react';
import Link from 'next/link';
import { Bell } from 'lucide-react';

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5258';

interface NotificationFormat {
    id: string;
    employerName: string;
    notificationType: string;
    workerInfoDesc: string;
    notificationTime: string;
}

const formatTime = (iso: string) =>
    new Date(iso).toLocaleString('en-US', {
        year: 'numeric', month: 'short', day: 'numeric',
        hour: '2-digit', minute: '2-digit', second: '2-digit',
        hour12: false
    });

export default function NotificationBell() {
    const [notifications, setNotifications] = useState<NotificationFormat[]>([]);
    const [isOpen, setIsOpen] = useState(false);

    useEffect(() => {
        const load = async () => {
            const workerId = localStorage.getItem('userId');
            if (!workerId) return;
            const token = localStorage.getItem('accessToken');
            if (!token) return;

            const res = await fetch(`${API_URL}/api/notification/unread/${workerId}`, {
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                }
            });
            if (!res.ok) return;

            const response = await res.json();
            setNotifications(response.data ?? []);
        };
        load();
    }, []);

    async function markAsRead(notificationId: string) {
        const token = localStorage.getItem('accessToken');
        const res = await fetch(`${API_URL}/api/notification/${notificationId}`, {
            method: 'PATCH',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            }
        });
        if (!res.ok) return;
        setNotifications(prev => prev.filter(n => n.id !== notificationId));
    }

    return (
        <div className="relative">
            {/* Bell button */}
            <button
                onClick={() => setIsOpen(prev => !prev)}
                className="relative p-2 rounded-full hover:bg-gray-100 transition-colors"
            >
                <Bell size={22} className="text-gray-600" />
                {notifications.length > 0 && (
                    <span className="absolute top-1 right-1 w-4 h-4 bg-red-500 text-white text-[10px] font-bold rounded-full flex items-center justify-center">
                        {notifications.length}
                    </span>
                )}
            </button>

            {/* Dropdown */}
            {isOpen && (
                <div className="absolute right-0 mt-2 w-80 bg-white rounded-xl shadow-xl border border-gray-100 z-50 flex flex-col">
                    {/* Header */}
                    <div className="px-4 py-3 border-b border-gray-100">
                        <p className="text-sm font-semibold text-gray-800">Notifications</p>
                        {notifications.length > 0 && (
                            <p className="text-xs text-gray-400 mt-0.5">{notifications.length} unread</p>
                        )}
                    </div>

                    {/* Items */}
                    <div className="max-h-72 overflow-y-auto divide-y divide-gray-50">
                        {notifications.length === 0 ? (
                            <p className="px-4 py-6 text-sm text-center text-gray-400">
                                No unread notifications
                            </p>
                        ) : (
                            notifications.map(n => (
                                <div
                                    key={n.id}
                                    onClick={() => markAsRead(n.id)}
                                    className="flex items-start gap-3 px-4 py-3 hover:bg-gray-50 cursor-pointer transition-colors"
                                >
                                    <span className="mt-1.5 w-2 h-2 flex-shrink-0 rounded-full bg-red-500" />
                                    <div className="min-w-0">
                                        <p className="text-sm font-medium text-gray-800 leading-snug">
                                            [{n.notificationType}] {n.employerName} — [{n.workerInfoDesc}]
                                        </p>
                                        <p className="text-xs text-gray-400 mt-0.5">
                                            {formatTime(n.notificationTime)}
                                        </p>
                                    </div>
                                </div>
                            ))
                        )}
                    </div>

                    {/* Footer */}
                    <div className="border-t border-gray-100 px-4 py-2.5">
                        <Link
                            href="/notification/all"
                            className="text-xs font-medium text-blue-500 hover:text-blue-600 transition-colors"
                        >
                            View all notifications →
                        </Link>
                    </div>
                </div>
            )}
        </div>
    );
}
