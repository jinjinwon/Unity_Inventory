<?php
	$u_id = $_POST["Input_user"];
	$my_gold = $_POST["My_Gold"];
	$item_list = $_POST["item_list"];

	$con = mysqli_connect("localhost", "jinone12", "wlsdnjs12!!", "jinone12");

	if(!$con)
		die("Could not Connect".mysqli_connect_error());

	$check = mysqli_query($con, "SELECT user_id FROM GawiBawiBo WHERE user_id ='".$u_id."'");

	$numrows = mysqli_num_rows($check);

	if(!$check || $numrows == 0)
		die("ID Does Exist.");
	
	if($row = mysqli_fetch_assoc($check))
	{
		mysqli_query($con, "UPDATE GawiBawiBo SET `my_gold`='".$my_gold."' WHERE `user_id`='".$u_id."'");
		mysqli_query($con, "UPDATE GawiBawiBo SET `my_info`='".$item_list."' WHERE `user_id`='".$u_id."'");
		echo("UpdateSuccess");
	}
	mysqli_close($con);
?>