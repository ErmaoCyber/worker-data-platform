'use client'

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';

export default function LandingPage() {

    return (
        <div className="text-center bg-blue-80 min-h-screen">
            <h1 className="text-4xl font-bold mt-32">Worker Data Blockchain</h1>
            <p className="text-xl font-bold mt-12 text-violet-300">Hi team! Welcome to Worker Data Blockchain.
                <br />Excited to build this together with you all — let's make it happen!</p>
        </div>
    );

}