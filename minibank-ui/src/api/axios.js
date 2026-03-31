import axios from 'axios';

const api = axios.create({
    baseURL: 'http://10.81.172.207:80/api'
});
export default api;