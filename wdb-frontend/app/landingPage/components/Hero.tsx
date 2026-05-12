// app/landingPage/components/Hero.tsx
import Image from "next/image";
import Link from "next/link";
import CompanyLogo from "./../../assets/CompanyLogo";
import heroBackground from "./../../assets/heroBackground.png";
import blockchainLogo from "./../../assets/blockchainLogo.png";
import firstStepSolutions from "./../../assets/firstStepSolutions.png";

export default function Hero() {
    return (
        <div className="relative h-screen flex flex-col bg-[#374151] snap-start shrink-0">
            <div className="absolute inset-0" />

            <Image
                src={heroBackground}
                alt=""
                fill
                className="object-cover object-center opacity-30"
                priority
            />

            {/* Logo */}
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
                        Gain full control of your professional information with{" "}
                        <span className="font-bold text-xl">workPass</span>. <br />
                        Enhanced with blockchain technology.
                    </p>
                    <div className="flex items-center gap-3 mt-2">
                        <Link href="/login">
                            <button className="cursor-pointer bg-gray-200 text-gray-700 font-bold text-sm tracking-wide px-7 py-3.5 rounded-2xl hover:bg-yellow-200 transition-opacity">
                                Get Started
                            </button>
                        </Link>
                    </div>
                </div>
            </section>

            {/* Bottom logos */}
            <div className="absolute bottom-16 left-0 right-0 z-10 flex justify-center items-center gap-8">
                <Image
                    src={firstStepSolutions}
                    alt=""
                    width={0}
                    height={0}
                    style={{ height: '40px', width: 'auto' }}
                    priority
                />
                <Image
                    src={blockchainLogo}
                    alt=""
                    width={0}
                    height={0}
                    style={{ height: '60px', width: 'auto' }}
                    priority
                />
            </div>
        </div>
    );
}