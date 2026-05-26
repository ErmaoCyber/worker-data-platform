'use client';

import { createContext, ReactNode, useContext, useEffect, useState } from 'react';

// Auth data stored in React state
type AuthState = {
    token: string | null;
    role: string | null;
    userName: string | null;
    email: string | null;
    userId: string | null;
};

// Auth context exposed to the app
type AuthContextType = AuthState & {
    isAuthReady: boolean;
    login: (token: string, userName: string, email: string, userId: string, role: string) => void;
    logout: () => void;
};

type AuthProviderProps = {
    children: ReactNode;
};

const emptyAuthState: AuthState = {
    token: null,
    role: null,
    userName: null,
    email: null,
    userId: null,
};

export const AuthContext = createContext<AuthContextType | null>(null);

export const AuthProvider = ({ children }: AuthProviderProps) => {
    const [authState, setAuthState] = useState<AuthState>(emptyAuthState);

    // Used to prevent protected pages from redirecting before localStorage is checked
    const [isAuthReady, setIsAuthReady] = useState(false);

    useEffect(() => {
        // Restore auth state after page refresh
        const storedToken = localStorage.getItem('accessToken');
        const storedRole = localStorage.getItem('role');
        const storedUserName = localStorage.getItem('userName');
        const storedEmail = localStorage.getItem('email');
        const storedUserId = localStorage.getItem('userId');

        if (storedToken && storedRole && storedUserName && storedEmail && storedUserId) {
            // This is intentional: we restore auth state once from localStorage on mount.
            // eslint-disable-next-line react-hooks/set-state-in-effect
            setAuthState({
                token: storedToken,
                role: storedRole,
                userName: storedUserName,
                email: storedEmail,
                userId: storedUserId,
            });
        }

        // Auth restoration has finished, even if no saved login exists
        setIsAuthReady(true);
    }, []);

    const login = (
        token: string,
        userName: string,
        email: string,
        userId: string,
        role: string
    ) => {
        const newAuthState: AuthState = {
            token,
            role,
            userName,
            email,
            userId,
        };

        // Update React state
        setAuthState(newAuthState);

        // Persist login after refresh
        localStorage.setItem('accessToken', token);
        localStorage.setItem('role', role);
        localStorage.setItem('userName', userName);
        localStorage.setItem('email', email);
        localStorage.setItem('userId', userId);

        setIsAuthReady(true);
    };

    const logout = () => {
        // Clear React state
        setAuthState(emptyAuthState);

        // Clear saved login
        localStorage.removeItem('accessToken');
        localStorage.removeItem('role');
        localStorage.removeItem('userName');
        localStorage.removeItem('email');
        localStorage.removeItem('userId');

        setIsAuthReady(true);
    };

    return (
        <AuthContext.Provider
            value={{
                token: authState.token,
                role: authState.role,
                userName: authState.userName,
                email: authState.email,
                userId: authState.userId,
                isAuthReady,
                login,
                logout,
            }}
        >
            {children}
        </AuthContext.Provider>
    );
};

// Custom hook for reading auth context
export const useAuth = () => {
    const context = useContext(AuthContext);

    if (!context) {
        throw new Error('useAuth must be used within an AuthProvider');
    }

    return context;
};