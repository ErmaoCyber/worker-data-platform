'use client';

import * as signalR from '@microsoft/signalr';
import { useEffect } from 'react';
import { toast, ToastContainer } from 'react-toastify';
import { useUser } from '@/lib/api/userContext';

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5258';

export default function SignalRListener(){

    // Consume centralized user state so the listener re-subscribes when the user logs in.
    // Root layout never unmounts, so reading localStorage once on mount missed post-login changes.
    const { user } = useUser();
    const workerId = user?.userId;

    useEffect(() => {
        // Previously read workerId from localStorage on mount only; replaced by the
        // context value above so this effect can re-run when userId changes.
        // const workerId = localStorage.getItem("userId");

        // Skip until a user is present; the effect re-runs after login when workerId becomes set.
        if (!workerId) return;

        // create the signalR connection, call the URL of notification hub
        const connection = new signalR.HubConnectionBuilder().withUrl(`${API_URL}/hubs/notifications?workerId=${workerId}`).withAutomaticReconnect().build();

        // Register the handler before start() so a message arriving during the start
        // handshake is not dropped.
        connection.on("NotificationInfo", (message) => {
            toast.info(message);
        });

        // when connected
        // connection.start().then(() =>{
        //     connection.on("NotificationInfo", (message) =>{
        //         toast.info(message);
        //     });
        // }).catch(console.error);
        connection.start().catch(console.error);

        return () => { connection.stop(); };

    }, [workerId]);

    return <ToastContainer/>


}