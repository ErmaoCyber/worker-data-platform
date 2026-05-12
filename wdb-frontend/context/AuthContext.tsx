'use client';
// 改这一行
import { createContext, useState, ReactNode, useEffect, useContext } from 'react';


// define the type for the authentication context
type AuthContextType = {
    token: string | null;
    role: string | null;
    userName: string | null;
    email: string | null;
    userId: string | null;
    login: (token: string, userName: string, email: string, userId: string, role: string) => void;
    logout: () => void;
};

export const AuthContext = createContext<AuthContextType | null>(null); // create a context with the defined type

// create a provider component to wrap the app and provide the authentication state
// first, define the type for the provider's props
type AuthProviderProps = {
    children: ReactNode;
};

// then, create the provider component
export const AuthProvider = ({ children }: AuthProviderProps) => {
    const [token, setToken] = useState<string | null>(null);
    const [role, setRole] = useState<string | null>(null);
    const [userName, setUserName] = useState<string | null>(null);
    const [email, setEmail] = useState<string | null>(null);
    const [userId, setUserId] = useState<string | null>(null);

    useEffect(() => {
        // Load auth state from localStorage on mount
        const storedToken = localStorage.getItem('accessToken');
        const storedRole = localStorage.getItem('role');
        const storedUserName = localStorage.getItem('userName');
        const storedEmail = localStorage.getItem('email');
        const storedUserId = localStorage.getItem('userId');
        if (storedToken && storedRole && storedUserName && storedEmail && storedUserId) {
            setToken(storedToken);
            setRole(storedRole);
            setUserName(storedUserName);
            setEmail(storedEmail);
            setUserId(storedUserId);
        }
    }, []);


    // define the login and logout functions to update the authentication state
    const login = (token: string, userName: string, email: string, userId: string, role: string) => {
        setToken(token);
        setUserId(userId);
        setUserName(userName);
        setEmail(email);
        setRole(role);
        localStorage.setItem('accessToken', token);
        localStorage.setItem('userName', userName);
        localStorage.setItem('email', email);
        localStorage.setItem('userId', userId);
        localStorage.setItem('role', role);
    };
    // the logout function clarifies the authentication state and removes the relevant items form localstorage
    const logout = () => {
        setToken(null);
        setRole(null);
        setUserName(null);
        setEmail(null);
        setUserId(null);
        localStorage.removeItem('accessToken');
        localStorage.removeItem('role');
        localStorage.removeItem('userName');
        localStorage.removeItem('email');
        localStorage.removeItem('userId');
    };

    // return the provider component with the authentication state and functions as the value
    return (
        <AuthContext.Provider value={{ token, role, userName, email, userId, login, logout }}>
            {children}
        </AuthContext.Provider>
    );

};
// create a custom hook to use the authentication context in other components
export const useAuth = () => {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error('useAuth must be used within an AuthProvider');
    }
    return context;

}