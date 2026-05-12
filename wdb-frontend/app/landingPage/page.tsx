'use client'

import Image from "next/image";
import CompanyLogo from "./../assets/CompanyLogo";
import heroBackground from "./../assets/heroBackground.png";
import howitWorks from "./../assets/howitWorks.png";
import blockchainLogo from "./../assets/blockchainLogo.png";

import Link from "next/link";

import HowItWorks from "./components/HowItWorks";
import FAQ from "./components/FAQ";
import Gallery from "./components/Gallery";


export default function LandingPage() {
    return (
        <main className="flex flex-col bg-gray-900 text-gray-300">

            {/* ── Hero (full viewport) ── */}
            <div className="relative min-h-screen flex flex-col bg-[#374151]">
                <div className="absolute inset-0 " />

                {/* Background image scoped to hero only */}
                <Image
                    src={heroBackground}
                    alt=""
                    fill
                    className="object-cover object-center opacity-30"
                    priority

                />


                {/* Nav */}
                <nav className="relative z-10 flex items-center px-10 py-7">
                    <CompanyLogo height={30} />
                </nav>

                {/* Hero content */}
                <section className="relative z-10 flex flex-1 items-center justify-center px-6">
                    <div className="flex flex-col items-center text-center gap-6 max-w-2xl animate-fadeUp">
                        <h1 className="font-extrabold text-white leading-[1.05] tracking-[-0.03em] text-[clamp(44px,7vw,48px)]">
                            Your Data,
                            <br />
                            Your Sovereignty
                        </h1>
                        <p className="text-[17px] font-light leading-relaxed text-gray-200">
                            Gain full control of your professional information with workPass. <br />
                            Enhanced with blockchain technology.
                        </p>
                        <div className="flex items-center gap-3 mt-2">
                            <Link href="/login">
                                <button className="cursor-pointer bg-gray-200 text-gray-700 font-bold text-sm tracking-wide px-7 py-3.5 rounded-2xl hover:opacity-85 transition-opacity">
                                    Get Started
                                </button>
                            </Link>
                        </div>

                    </div>
                </section>
                <div className="absolute bottom-16 left-0 right-0 z-10 flex justify-center">
                    <Image
                        src={blockchainLogo}
                        alt=""
                        width={160}
                        priority
                    />
                </div>
            </div>

            {/*Section 2 Gallery */}
            <section className="py-24 px-10 flex flex-col items-center text-center gap-6">
                <Gallery />
            </section>

            {/*Section 3 : How it works */}
            <section className="py-24 px-10 bg-white flex flex-col items-center">
                <h2 className="text-4xl font-bold text-slate-600">How <span className="font-normal">workPass</span> Works</h2>
                <HowItWorks />
            </section>

            {/*Section 4 : FAQs*/}
            <section className="py-24 px-10 bg-white flex flex-col items-center text-center gap-6">
                <FAQ />
            </section>



        </main>
    );
}