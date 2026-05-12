
'use client'

import Hero from "./components/Hero";
import Gallery from "./components/Gallery";
import HowItWorks from "./components/HowItWorks";
import FAQ from "./components/FAQ";

export default function LandingPage() {
    return (
        <main className="flex flex-col bg-gray-900 text-gray-300 snap-y snap-mandatory overflow-y-scroll h-screen">

            <Hero />

            <section className="h-screen snap-start shrink-0">
                <Gallery />
            </section>

            <section className="py-24 px-10 bg-white h-screen snap-start shrink-0 overflow-y-auto">
                <HowItWorks />
            </section>

            <section className="py-24 px-10 bg-white h-screen snap-start shrink-0 overflow-y-auto">
                <FAQ />
            </section>

        </main>
    );
}