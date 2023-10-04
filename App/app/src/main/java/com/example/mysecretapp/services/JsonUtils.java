package com.example.mysecretapp.services;

import android.text.TextUtils;
import android.util.Log;

import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.ObjectMapper;

import java.io.BufferedInputStream;
import java.io.BufferedReader;
import java.io.DataOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.URL;

public class JsonUtils {

    public static <T> T PostJson(Object object, String url, TypeReference<T> typeReference) throws IOException {

        HttpURLConnection conn = null;
        ObjectMapper mapper = new ObjectMapper();
        try {


            URL urlObj = new URL(url);
            conn = (HttpURLConnection) urlObj.openConnection();
            conn.setDoOutput(true);
            conn.setRequestMethod("POST");
            conn.setRequestProperty("Accept-Charset", "UTF-8");
            conn.setRequestProperty("Content-Type", "application/json");
            conn.setRequestProperty("Accept", "application/json");
            conn.setRequestProperty("Cookie","AgentID=1");
            conn.setReadTimeout(5000);
            conn.setConnectTimeout(5000);

            conn.connect();
            if (object != null) {
                String json = mapper.writeValueAsString(object);
                System.out.println(json);
                System.out.println(url);
                DataOutputStream wr = new DataOutputStream(conn.getOutputStream());
                wr.writeBytes(json);
                wr.flush();
                wr.close();
            }
            InputStream in = new BufferedInputStream(conn.getInputStream());
            BufferedReader reader = new BufferedReader(new InputStreamReader(in));
            StringBuilder result = new StringBuilder();
            String line;
            while ((line = reader.readLine()) != null) {
                result.append(line);
            }

            String json = result.toString();
            if (json.isEmpty()) {
                return null;
            }
                T grant = mapper.readValue(json, typeReference);


            Log.d("test", "result from server: " + result.toString());
            return grant;


        } finally {
            if (conn != null) {
                conn.disconnect();
            }
        }

    }
}
