'use client';

import * as signalR from '@microsoft/signalr';
import { useEffect } from 'react';
import { toast, ToastContainer } from 'react-toastify';

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5258';

export default function SignalRListener(){

    useEffect(() => {
        // get the workerId from the browser
        const workerId = localStorage.getItem("userId");
        
        // create the signalR connection, call the URL of notification hub
        const connection = new signalR.HubConnectionBuilder().withUrl(`${API_URL}/hubs/notifications?workerId=${workerId}`).withAutomaticReconnect().build();

        // when connected
        connection.start().then(() =>{
            connection.on("NotificationInfo", (message) =>{
                toast.info(message);
            });
        }).catch(console.error);

        return () => { connection.stop(); };

    }, []);

    return <ToastContainer/>


}