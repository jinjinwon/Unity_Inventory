<?php
		// Unity Import
		$u_id = $_POST[ "Input_user" ];
		$u_pw = $_POST[ "Input_pass" ];
		$nick = $_POST[ "Input_nick" ];

		$con = mysqli_connect("localhost","jinone12","wlsdnjs12!!","jinone12");
		
		if(!$con)
			die("Could not Connect".mysqli_connect_Error());
		echo $u_id."<br>";
		echo $u_pw."<br>";
		echo $nick."<br>";
		echo "DB 접속 완료";	

		$check = mysqli_query($con, "SELECT user_id FROM 3DPortfolio WHERE user_id ='".$u_id."'");
		$numrows = mysqli_num_rows($check);		
		if($numrows != 0)
		{
			die("ID Does Exist. \n");
		}		
		
		$check = mysqli_query($con, "SELECT nick_name FROM 3DPortfolio WHERE nick_name ='".$nick."'");
		$numrows = mysqli_num_rows($check);
		
		if($numrows != 0)
		{
			die("Nick Does Exist. \n");
		}	
		
		$Result = mysqli_query($con, "INSERT INTO 3DPortfolio (user_id, user_pw, nick_name) VALUES('".$u_id."','".$u_pw."','".$nick."');");
		if($Result)
			die("Create Success. \n");
		else
			die("Create Error. \n");
	mysqli_close($con);				
?>