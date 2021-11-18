<?php
	$u_id = $_POST["Input_user"]; 
	$score = $_POST["Input_score"];

	$con = mysqli_connect("localhost", "jinone12", "wlsdnjs12!!", "jinone12");

	if(!$con)
		die("Could not Connect".mysqli_connect_error());
	$check = mysqli_query($con, "SELECT user_id FROM GawiBawiBo WHERE user_id ='".$u_id."'");

	$numrows = mysqli_num_rows($check);
	if($numrows == 0)
	{  		
		die("ID Does Exist.");
	} 
	if($row = mysqli_fetch_assoc($check))
	{
		mysqli_query($con, "UPDATE GawiBawiBo SET `best_score`='".$score."' WHERE `user_id`='".$u_id."'");
		echo("UpDataSuccess");
	}
	mysqli_close($con);
?>