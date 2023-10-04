package com.example.mysecretapp;



public class Commons {
    public static String PrivatePreference ="PrivatePreference";
    public static String EMAIL_KEY="email";
    public static String PASSWORD_KEY="password";
    public static class WebApplicationEndPoints
    {

        // private static String IpEndpoint="172.16.208.97:5000";
       private static String IpEndpoint="192.168.1.109:5001";


        private static String BaseEndpoint="http://"+IpEndpoint+"/api/controller/app/";

        public static String LoginEndpoint=BaseEndpoint+"login";
        public static String ListSecretsEndpoint=BaseEndpoint+"listsecrets";

        public static String InitEndpoint=BaseEndpoint+"init";


    }
}
