import React, { useEffect } from 'react';
import auth from './auth';

const Login = () => {
  useEffect(() => {
    auth.login(); 
  }, []);

  return <div>Redirection vers la page de connexion...</div>;
};

export default Login;
