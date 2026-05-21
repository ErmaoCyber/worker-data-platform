'use client';

import * as signalR from '@microsoft/signalr';
import { useEffect } from 'react';
import { toast, ToastContainer } from 'react-toastify';
import { useAuth } from '@/context/AuthContext';

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5258';

export default function SignalRListener(){

    // Consume centralized auth state so the listener re-subscribes when the user logs in.
    // Root layout never unmounts, so reading localStorage once on mount would miss post-login changes.
    const { userId, isAuthReady } = useAuth();

    useEffect(() => {
        // Wait until AuthContext finishes restoring auth state from localStorage,
        // then skip until a user is actually present.
        if (!isAuthReady || !userId) return;

        const connection = new signalR.HubConnectionBuilder().withUrl(`${API_URL}/hubs/notifications?workerId=${userId}`).withAutomaticReconnect().build();

        // Register the handler before start() so a message arriving during the start
        // handshake is not dropped.
        connection.on("NotificationInfo", (message) => {
            toast.info(message);
        });

        connection.start().catch(console.error);

        return () => { connection.stop(); };

    }, [isAuthReady, userId]);

    return <ToastContainer/>


}
