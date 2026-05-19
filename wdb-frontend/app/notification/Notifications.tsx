'use client'

import { useEffect, useState } from "react";

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5258';

interface NotifcationFormat{
    id: string,
    employerName: string,
    workerName: string,
    notificationType: string,
    workerInfoDesc: string,
    notificationTime: string
}

// format ISO date string to 24-hour format e.g. "May 14, 2026, 10:23:45"
const formatTime = (iso: string) =>
    new Date(iso).toLocaleString('en-US', {
        year: 'numeric', month: 'short', day: 'numeric',
        hour: '2-digit', minute: '2-digit', second: '2-digit',
        hour12: false
    });

export default function Notification(){

    const [notifications, setNotifications] = useState<NotifcationFormat[]>([]);
    // const [refreshKey, setRefreshKey] = useState(0);

    // const workerId = localStorage.getItem("userId");

    useEffect(() => {
      const load = async () => {
          const workerId = localStorage.getItem("userId");
          const token = localStorage.getItem("accessToken");
          // skip request if user is not logged in
          if (!token) return;

          const res = await fetch(`${API_URL}/api/notification/unread/${workerId}`, {
              method: 'GET',
              headers: {
                  'Content-Type': 'application/json',
                  'Authorization': `Bearer ${token}`
              }
          });
          // skip parsing if response is not successful (e.g. 401, 500)
          if (!res.ok) return;

          const response = await res.json();
          setNotifications(response.data ?? []);
      };
      load();
    }, []);

    async function updateNotificationStatusHandler(notificationId: string) {
        const token = localStorage.getItem("accessToken");
        
        const res = await fetch(`${API_URL}/api/notification/${notificationId}`, {
            method: 'PATCH',
            headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`,
            },
        });

        if (!res.ok) {
            console.error('Update failed:', res.status);
            return;
        }

        // refresh
        // setRefreshKey(k => k + 1);
        setNotifications(prev => prev.filter(n => n.id !== notificationId));
    }

    return (
        <div>
            {notifications.map(n => (
                <p key={n.id} onClick={() => updateNotificationStatusHandler(n.id)}>
                    {n.notificationType + ": " + n.employerName + " - " + n.workerInfoDesc + " at " + formatTime(n.notificationTime)}
                </p>
            ))}
        </div>
    );

}