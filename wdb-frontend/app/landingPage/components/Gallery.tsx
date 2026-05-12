'use client'

import { useState } from "react";
import Image, { StaticImageData } from "next/image";

// Import your images here
import slide1 from "@/app/landingPage/assets/slide1.png";
import slide2 from "@/app/landingPage/assets/slide2.png";
import slide3 from "@/app/landingPage/assets/slide3.png";

interface Slide {
    image: StaticImageData;
    problem: string;
    solutionTitle: string;
    solutionDescription: string;
}

const slides: Slide[] = [
    {
        image: slide1,
        problem: "Repetitive paperwork",
        solutionTitle: "Centralized Work Profile",
        solutionDescription: "Maintain a single source of truth for your professional information.",
    },
    {
        image: slide2,
        problem: "Lack of data transparency",
        solutionTitle: "Blockchain-Verified Records",
        solutionDescription: "Every access and transaction is permanently recorded and fully auditable.",
    },
    {
        image: slide3,
        problem: "Unauthorized data access",
        solutionTitle: "Permission-Based Access Control",
        solutionDescription: "You decide who sees your data, when they see it, and for how long.",
    },
];

export default function Gallery() {
    const [current, setCurrent] = useState(0);

    const prev = () => setCurrent((i) => (i === 0 ? slides.length - 1 : i - 1));
    const next = () => setCurrent((i) => (i === slides.length - 1 ? 0 : i + 1));

    const slide = slides[current];

    return (
        <section className="w-full bg-white border-y border-gray-200">
            <div className="relative w-full flex items-center justify-between px-6 py-16">

                {/* Left arrow */}
                <button
                    onClick={prev}
                    className="z-10 shrink-0 w-10 h-10 flex items-center justify-center rounded-full border border-gray-300 text-gray-500 hover:bg-gray-100 transition-colors"
                >
                    ‹
                </button>

                {/* Slide content */}
                <div className="flex flex-1 items-center justify-between px-10 gap-10">

                    {/* Left — image + problem tag */}
                    <div className="relative flex items-center justify-center w-1/2">

                        {/* Problem tag — top left */}
                        <span className="absolute top-0 left-0 bg-yellow-200 text-gray-700 text-sm font-medium px-4 py-2 rounded-md">
                            {slide.problem}
                        </span>

                        <Image
                            src={slide.image}
                            alt={slide.solutionTitle}
                            height={280}
                            className="object-contain mt-10"
                        />
                    </div>

                    {/* Right — solution */}
                    <div className="flex flex-col gap-3 w-1/2">
                        <h2 className="text-3xl font-bold text-gray-800">
                            {slide.solutionTitle}
                        </h2>
                        <p className="text-sm text-gray-500 leading-relaxed">
                            {slide.solutionDescription}
                        </p>
                    </div>

                </div>

                {/* Right arrow */}
                <button
                    onClick={next}
                    className="z-10 shrink-0 w-10 h-10 flex items-center justify-center rounded-full border border-gray-300 text-gray-500 hover:bg-gray-100 transition-colors"
                >
                    ›
                </button>

            </div>

            {/* Dots */}
            <div className="flex justify-center gap-2 pb-8">
                {slides.map((_, i) => (
                    <button
                        key={i}
                        onClick={() => setCurrent(i)}
                        className={`w-2.5 h-2.5 rounded-full transition-colors ${i === current ? "bg-gray-700" : "bg-gray-300"
                            }`}
                    />
                ))}
            </div>

        </section>
    );
}