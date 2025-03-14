import React, { useEffect } from 'react';
import auth from './auth';

const Callback = () => {
  useEffect(() => {
    auth.signinCallback()
      .then((user) => {
        console.log(user);
        window.location.href = '/'; 
      })
      .catch((err) => {
        console.error(err);
      });
  }, []);

  return <div>Chargement...</div>;
};

export default Callback;
