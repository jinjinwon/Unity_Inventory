<?php
	$u_id = $_POST["Input_user"]; 
	$user_Item = $_POST["User_Item"];

	$con = mysqli_connect("localhost", "jinone12", "wlsdnjs12!!", "jinone12");

	if(!$con)
		die("Could not Connect".mysqli_connect_error());
	$check = mysqli_query($con, "SELECT user_id FROM 3DPortfolio WHERE user_id ='".$u_id."'");

	$numrows = mysqli_num_rows($check);
	if($numrows == 0)
	{  		
		die("ID Does Exist.");
	} 
	if($row = mysqli_fetch_assoc($check))
	{
		mysqli_query($con, "UPDATE 3DPortfolio SET `user_item`='".$user_Item."' WHERE `user_id`='".$u_id."'");
		echo("UpdateSuccess");
	}
	mysqli_close($con);
?>