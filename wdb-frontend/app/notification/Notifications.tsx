'use client'

import { FetchApi } from "@/lib/api";
import React, { useEffect } from "react";
import { useState } from "react";

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5258';

interface NotifcationFormat{
    id: string,
    employerName: string,
    workerName: string,
    notificationType: string,
    workerInfoDesc: string,
    notificationTime: string
}

export default function Notification(){

    const [notifications, setNotifications] = useState<NotifcationFormat[]>([]);
    // const [refreshKey, setRefreshKey] = useState(0);

    // const workerId = localStorage.getItem("userId");
    const workerId = "019de156-fc1a-7770-ad95-e895fa39cdd3"; 

    useEffect(() => {
      const load = async () => {
          const token = localStorage.getItem("accessToken");
          const res = await fetch(`${API_URL}/api/notification/unread/${workerId}`, {
              method: 'GET',
              headers: {
                  'Content-Type': 'application/json',
                  'Authorization': `Bearer ${token}`
              }
          });
          const response = await res.json();
        //   console.log(response.data);
          setNotifications(response.data);
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
            {notifications.map(n => <p key={n.id} onClick={() => updateNotificationStatusHandler(n.id)}>{n.notificationType + ": " + n.employerName + " at " + n.notificationTime}</p>)}
        </div>
    );

}