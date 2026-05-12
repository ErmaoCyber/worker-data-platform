'use client'

import { useState, useEffect } from "react";
import { StaticImageData } from "next/image";
import { ReactNode } from "react";


import centralized from "./../../assets/slides/centralized.png";
import consent from "./../../assets/slides/consent.png";
import control from "./../../assets/slides/control.png";
import blockchain from "./../../assets/slides/blockchain.png";

interface Slide {
    image: StaticImageData;
    problem: ReactNode;
    solutionTitle: ReactNode;
    solutionDescription: ReactNode;
    darkBackground?: boolean,
}

const slides: Slide[] = [
    {
        image: centralized,
        problem: <>Repetitive paperwork</>,
        solutionTitle: <>Centralized Work Profile</>,
        solutionDescription: <>Maintain a single source of truth for your professional information.</>,
        darkBackground: false,
    },
    {
        image: consent,
        problem: <>Data access without consent</>,
        solutionTitle: <>Consent-Based Access</>,
        solutionDescription: <>No employer gains access automatically. Every request requires your explicit consent before anything is shared.</>,
        darkBackground: true,
    },
    {
        image: control,
        problem: <>Zero control after submission</>,
        solutionTitle: <>Full Access Control</>,
        solutionDescription: <>Set expiry date for data access or revoke access anytime.</>,
        darkBackground: false,
    },
    {
        image: blockchain,
        problem: <>No visibility over data access</>,
        solutionTitle: <>Tamper-Proof Audit Trail</>,
        solutionDescription: <>Utilizing blockchain technology in ensuring all transaction <br /> is recorded and can't be changed by any party. <br /> Providing full transparency and accountability.</>,
        darkBackground: true,
    },
];

export default function Gallery() {
    const [current, setCurrent] = useState(0);

    const prev = () => setCurrent((i) => (i === 0 ? slides.length - 1 : i - 1));
    const next = () => setCurrent((i) => (i === slides.length - 1 ? 0 : i + 1));

    {/*Auto Slide */ }
    useEffect(() => {
        const timer = setInterval(() => {
            setCurrent((i) => (i === slides.length - 1 ? 0 : i + 1));
        }, 4000);

        return () => clearInterval(timer);
    }, []);

    const slide = slides[current];
    const dark = slide.darkBackground ?? true;

    return (
        <section className="w-full">
            <div className="relative w-full h-screen flex items-center justify-between px-6 bg-fixed">

                {/* Stacked background layers */}
                {slides.map((s, i) => (
                    <div
                        key={i}
                        className="absolute inset-0 bg-cover bg-center transition-opacity duration-500"
                        style={{
                            backgroundImage: `url(${s.image.src})`,
                            opacity: i === current ? 1 : 0,
                        }}
                    />
                ))}

                {/* Overlay to make text appear clearer — only on dark backgrounds */}
                {dark && <div className="absolute inset-0 bg-black/70 transition-opacity duration-500" />}

                {/* Left arrow */}
                <button
                    onClick={prev}
                    className={`relative z-10 shrink-0 w-10 h-10 flex items-center justify-center rounded-full border transition-colors text-xl ${dark
                        ? "border-white/50 text-white hover:bg-white/20"
                        : "border-gray-400 text-gray-700 hover:bg-black/10"
                        }`}
                >
                    ‹
                </button>

                {/* Slide content */}
                {/* Problem */}
                <span className={`absolute top-24 left-0 z-10 text-2xl font-extrabold px-8 py-4 rounded-r-3xl flex items-center gap-3 ${dark ? "text-white" : "text-gray-700"} `}>
                    <span className="flex items-center justify-center w-8 h-8 rounded-full border-2 text-sm font-black shrink-0">
                        !
                    </span>
                    <span>{slide.problem}</span>
                </span>
                {/* Solutions*/}
                <div className="relative z-10 flex flex-1 flex-col justify-between h-full py-8 px-10">
                    <div className="absolute right-24 top-1/2 -translate-y-1/2 z-10 flex flex-col gap-2 text-right max-w-sm">
                        <h2 className={`text-5xl font-bold ${dark ? "text-white" : "text-gray-700"}`}>
                            {slide.solutionTitle}
                        </h2>
                        <p className={`text-sm leading-relaxed ${dark ? "text-gray-200" : "text-gray-600"}`}>
                            {slide.solutionDescription}
                        </p>
                    </div>

                </div>

                {/* Right arrow */}
                <button
                    onClick={next}
                    className={`relative z-10 shrink-0 w-10 h-10 flex items-center justify-center rounded-full border transition-colors text-xl ${dark
                        ? "border-white/50 text-white hover:bg-white/20"
                        : "border-gray-400 text-gray-700 hover:bg-black/10"
                        }`}
                >
                    ›
                </button>

                {/* Dots */}
                <div className="absolute bottom-4 left-0 right-0 z-10 flex justify-center gap-2">
                    {slides.map((_, i) => (
                        <button
                            key={i}
                            onClick={() => setCurrent(i)}
                            className={`w-2.5 h-2.5 rounded-full transition-colors ${i === current
                                ? dark ? "bg-white" : "bg-gray-800"
                                : dark ? "bg-white/40" : "bg-gray-400"
                                }`}
                        />
                    ))}
                </div>

            </div>
        </section>
    );
}