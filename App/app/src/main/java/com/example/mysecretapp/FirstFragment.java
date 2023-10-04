package com.example.mysecretapp;

import static java.nio.charset.StandardCharsets.UTF_8;

import android.graphics.Color;
import android.os.Build;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TableLayout;
import android.widget.TableRow;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.fragment.app.Fragment;

import com.example.mysecretapp.databinding.FragmentFirstBinding;
import com.example.mysecretapp.dto.EncryptedSecretsWrapper;
import com.example.mysecretapp.dto.InitDTO;
import com.example.mysecretapp.dto.LoginDTO;
import com.example.mysecretapp.dto.LoginResultDTO;
import com.example.mysecretapp.dto.MySecretDto;
import com.example.mysecretapp.dto.SecretsWrapper;
import com.example.mysecretapp.dto.TokenDto;
import com.example.mysecretapp.services.JsonUtils;
import com.example.mysecretapp.services.KeyHolder;
import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.ObjectMapper;

import java.io.Console;
import java.io.IOException;
import java.lang.reflect.Type;
import java.nio.charset.StandardCharsets;
import java.security.InvalidAlgorithmParameterException;
import java.security.InvalidKeyException;
import java.security.KeyFactory;
import java.security.KeyPair;
import java.security.KeyPairGenerator;
import java.security.NoSuchAlgorithmException;
import java.security.PrivateKey;
import java.security.PublicKey;
import java.security.spec.InvalidKeySpecException;
import java.security.spec.PKCS8EncodedKeySpec;
import java.security.spec.RSAPublicKeySpec;
import java.security.spec.X509EncodedKeySpec;
import java.util.Base64;
import java.util.Formatter;

import javax.crypto.BadPaddingException;
import javax.crypto.Cipher;
import javax.crypto.IllegalBlockSizeException;
import javax.crypto.NoSuchPaddingException;
import javax.crypto.SecretKey;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.SecretKeySpec;

public class FirstFragment extends Fragment {

    private FragmentFirstBinding binding;

    @Override
    public View onCreateView(
            LayoutInflater inflater, ViewGroup container,
            Bundle savedInstanceState
    ) {

        binding = FragmentFirstBinding.inflate(inflater, container, false);
        return binding.getRoot();

    }

    private SecretsWrapper CheckLogin(String email, String password) throws IOException, NoSuchAlgorithmException, NoSuchPaddingException, InvalidKeyException, IllegalBlockSizeException, BadPaddingException, InvalidKeySpecException, InvalidAlgorithmParameterException {

/************* INIT *******************/
        InitDTO initDtoDTO = new InitDTO();
        initDtoDTO.Id = "";

        byte[] serverPublicKey = JsonUtils.PostJson(initDtoDTO, Commons.WebApplicationEndPoints.InitEndpoint, new TypeReference<byte[]>() {
        });
        KeyFactory kf = KeyFactory.getInstance("RSA");
        PublicKey publicKey = kf.generatePublic(new X509EncodedKeySpec(serverPublicKey));
        KeyHolder.serverPublicKey = publicKey;

/********** LOGIN **********************/

        KeyPairGenerator kpg = KeyPairGenerator.getInstance("RSA");
        kpg.initialize(2048);
        KeyPair kp = kpg.generateKeyPair();
        PrivateKey aPrivate = kp.getPrivate();
        PublicKey aPublic = kp.getPublic();
        KeyHolder.privateKey = aPrivate;
        KeyHolder.publicKey = aPublic;

        LoginDTO loginDTO = new LoginDTO(email, password);
        loginDTO.Password = password;
        loginDTO.Username = email;
        loginDTO.PublicKey = aPublic.getEncoded();

        byte[] loginDTOArray = encrypt(loginDTO, KeyHolder.serverPublicKey);
        byte[] result = JsonUtils.PostJson(loginDTOArray, Commons.WebApplicationEndPoints.LoginEndpoint, new TypeReference<byte[]>() {
        });
        LoginResultDTO loginResultDTO = decrypt(result, KeyHolder.privateKey, new TypeReference<LoginResultDTO>() {
        });
        System.out.println(loginResultDTO.ResultCode);
        KeyHolder.SymmetricKey = loginResultDTO.SymmetricKey;

        /**************** LIST SECRETS****************/
        TokenDto tokenDto = new TokenDto();
        tokenDto.Username = email;
        byte[] tokenDtoArray = encryptSymmetric(tokenDto, KeyHolder.SymmetricKey);
        byte[] encrypted = JsonUtils.PostJson(tokenDtoArray, Commons.WebApplicationEndPoints.ListSecretsEndpoint, new TypeReference<byte[]>() {
        });

        SecretsWrapper secretsWrapper = decryptSymmetric(encrypted, KeyHolder.SymmetricKey, new TypeReference<SecretsWrapper>() {
        });
        System.out.println("Secrets Size:" + secretsWrapper.Secrets.size());
        return secretsWrapper;
    }

    private <T> T decryptSymmetric(byte[] encrypted, String symmetricKey, TypeReference<T> typeRef) throws IllegalBlockSizeException, BadPaddingException, IOException, NoSuchPaddingException, NoSuchAlgorithmException, InvalidAlgorithmParameterException, InvalidKeyException {
        String algorithm = "AES/CBC/PKCS5Padding";
        Cipher cipher = Cipher.getInstance(algorithm);
        byte[] iv = new byte[16];
        IvParameterSpec ivParameterSpec = new IvParameterSpec(iv);
        byte[] decodedKey = symmetricKey.getBytes(UTF_8);
// rebuild key using SecretKeySpec
        SecretKey originalKey = new SecretKeySpec(decodedKey, 0, decodedKey.length, "AES");
        cipher.init(Cipher.DECRYPT_MODE, originalKey, ivParameterSpec);
        byte[] plainText = null;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            plainText = cipher.doFinal(encrypted);
        }
        ObjectMapper mapper = new ObjectMapper();
        T object = mapper.readValue(plainText, typeRef);
        return object;

    }

    private byte[] encryptSymmetric(Object object, String symmetricKey) throws NoSuchPaddingException, NoSuchAlgorithmException, InvalidAlgorithmParameterException, InvalidKeyException, JsonProcessingException, IllegalBlockSizeException, BadPaddingException {
        String algorithm = "AES/CBC/PKCS5Padding";
        ObjectMapper mapper = new ObjectMapper();
        String json = mapper.writeValueAsString(object);
        byte[] jsonArray = json.getBytes(UTF_8);
        Cipher cipher = Cipher.getInstance(algorithm);
        byte[] iv = new byte[16];
        IvParameterSpec ivParameterSpec = new IvParameterSpec(iv);
        byte[] decodedKey = symmetricKey.getBytes(UTF_8);
// rebuild key using SecretKeySpec
        SecretKey originalKey = new SecretKeySpec(decodedKey, 0, decodedKey.length, "AES");
        cipher.init(Cipher.ENCRYPT_MODE, originalKey, ivParameterSpec);
        byte[] cipherText = cipher.doFinal(jsonArray);
        return cipherText;
    }

    private byte[] encrypt(Object object, PublicKey publicKey) throws JsonProcessingException, NoSuchPaddingException, NoSuchAlgorithmException, InvalidKeyException, IllegalBlockSizeException, BadPaddingException {
        ObjectMapper mapper = new ObjectMapper();
        String json = mapper.writeValueAsString(object);
        byte[] jsonArray = json.getBytes(UTF_8);
        Cipher cipher = Cipher.getInstance("RSA/ECB/PKCS1Padding");
        cipher.init(Cipher.ENCRYPT_MODE, publicKey);
        System.out.println("JsonArray Size:" + jsonArray.length);
        byte[] encrypted = cipher.doFinal(jsonArray);
        return encrypted;
    }

    private <T> T decrypt(byte[] encryped, PrivateKey privateKey, TypeReference<T> typeRef) throws NoSuchPaddingException, NoSuchAlgorithmException, InvalidKeyException, IllegalBlockSizeException, BadPaddingException, JsonProcessingException {
        ObjectMapper mapper = new ObjectMapper();
        Cipher cipher = Cipher.getInstance("RSA/ECB/PKCS1Padding");
        cipher.init(Cipher.DECRYPT_MODE, privateKey);
        byte[] jsonArray = cipher.doFinal(encryped);
        String decryptedJson = new String(jsonArray, UTF_8);

        T object = mapper.readValue(decryptedJson, typeRef);
        return object;
    }


    private String arrayToHexString(byte[] array) {
        Formatter formatter = new Formatter();
        for (byte b : array) {
            formatter.format("%02x", b);
        }
        String hex = formatter.toString();
        return hex;
    }

    public void onViewCreated(@NonNull View view, Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        binding.buttonFirst.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                try {
                    binding.SecretsTableID.removeAllViews();
                    SecretsWrapper secretsWrapper = CheckLogin("admin", "jj");
                    for (MySecretDto secretDto : secretsWrapper.Secrets) {
                        TableRow tr_head = new TableRow(view.getContext());

                        tr_head.setBackgroundColor(Color.WHITE);        // part1
                        tr_head.setLayoutParams(new TableRow.LayoutParams(
                                TableRow.LayoutParams.MATCH_PARENT,
                                TableRow.LayoutParams.WRAP_CONTENT));


                        TextView label_hello = new TextView(view.getContext());

                        label_hello.setText(secretDto.Name);
                        label_hello.setTextColor(Color.BLACK);          // part2
                        label_hello.setPadding(5, 5, 5, 5);
                        tr_head.addView(label_hello);// add the column to the table row here

                        TextView label_android = new TextView(view.getContext());    // part3

                        label_android.setText(secretDto.Value); // set the text for the header
                        label_android.setTextColor(Color.BLACK); // set the color
                        label_android.setPadding(5, 5, 5, 5); // set the padding (if required)
                        tr_head.addView(label_android); // add the column to the table row here
                        binding.SecretsTableID.addView(tr_head);
                    }

                } catch (Exception e) {
                    throw new RuntimeException(e);
                }
          /*      NavHostFragment.findNavController(FirstFragment.this)
                        .navigate(R.id.action_FirstFragment_to_SecondFragment);*/

            }
        });
    }

    @Override
    public void onDestroyView() {
        super.onDestroyView();
        binding = null;
    }

}